using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Implementations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Tests.Registration;

/// <summary>
/// Covers database-backed messaging, persistence registration, and runtime behavior.
/// </summary>
public sealed class MessagingDatabaseRegistrationTests
{
    /// <summary>
    /// Verifies that messaging startup initializes only the context used by its database stores.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_Startup_ShouldInitializeOnlyMessagingDatabaseContext()
    {
        var services = new ServiceCollection();
        var messagingInitializer = new RecordingMessagingDatabaseInitializer();
        var unrelatedInitializer = new RecordingDatabaseInitializer();
        services.AddSingleton<IDatabaseInitializer<RaycynixDatabaseContext>>(messagingInitializer);
        services.AddSingleton<IDatabaseInitializer>(unrelatedInitializer);
        services.AddRaycynixMessaging(BuildMessagingConfiguration())
            .AddDatabasePersistence();

        await using var provider = services.BuildServiceProvider(validateScopes: true);
        var initializationService = provider.GetServices<IHostedService>()
            .Single(service => service.GetType().Name == "MessagingDatabasePersistenceInitializationService");

        await initializationService.StartAsync(TestContext.Current.CancellationToken);

        messagingInitializer.CallCount.Should().Be(1);
        unrelatedInitializer.CallCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that database persistence options can be bound from configuration using the default section name.
    /// </summary>
    [Fact]
    public void AddDatabasePersistence_WithConfiguration_ShouldBindPersistenceConfiguration()
    {
        var services = new ServiceCollection();
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
            services.AddSingleton<ITransportMessagePublisher, RecordingTransportPublisher>();
            services.AddRaycynixDatabase(BuildDatabaseOptions(databasePath), registerCallerAssembly: false)
                .AddSqlite();
            services.AddRaycynixMessaging(BuildMessagingConfiguration())
                .AddDatabasePersistence(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["MessagingDatabasePersistenceConfiguration:InboxTableName"] = "custom_inbox",
                        ["MessagingDatabasePersistenceConfiguration:OutboxTableName"] = "custom_outbox",
                        ["MessagingDatabasePersistenceConfiguration:CleanupBatchSize"] = "250"
                    })
                    .Build());

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<MessagingDatabasePersistenceConfiguration>();
            options.InboxTableName.Should().Be("custom_inbox");
            options.OutboxTableName.Should().Be("custom_outbox");
            options.CleanupBatchSize.Should().Be(250);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that database-backed messaging persistence survives service-provider recreation for outbox entries.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldPersistOutboxEntriesAcrossProviders()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>()
                    .InitializeAsync(TestContext.Current.CancellationToken);
                await using var scope = provider.CreateAsyncScope();
                var store = scope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                await store.EnqueueAsync(CreateSerializedMessage("msg-1"), TestContext.Current.CancellationToken);
            }

            await using var reloadedProvider = BuildProvider(databasePath);
            await using var reloadedScope = reloadedProvider.CreateAsyncScope();
            var reloadedStore = reloadedScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
            var entry = await reloadedStore.GetAsync("msg-1", TestContext.Current.CancellationToken);

            entry.Should().NotBeNull();
            entry.Status.Should().Be(MessageOutboxStatus.Pending);
            entry.Message.Destination.Should().Be("orders.created");
            entry.Message.Payload.Should().Equal(System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-1\"}"));
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that the database-backed inbox state prevents duplicate processing across service-provider recreation.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldPersistInboxStateAcrossProviders()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>()
                    .InitializeAsync(TestContext.Current.CancellationToken);
                var state = provider.GetRequiredService<InboxHandlerState>();
                await using var scope = provider.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IIncomingMessageProcessor>();
                await processor.ProcessAsync(CreateIncomingMessage("msg-2"), TestContext.Current.CancellationToken);
                state.Values.Should().ContainSingle().Which.Should().Be("once");
            }

            await using (var provider = BuildProvider(databasePath))
            {
                var state = provider.GetRequiredService<InboxHandlerState>();
                await using var scope = provider.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IIncomingMessageProcessor>();
                await processor.ProcessAsync(CreateIncomingMessage("msg-2"), TestContext.Current.CancellationToken);
                state.Values.Should().BeEmpty();
            }
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that outbox failure state is recovered from persistent storage.
    /// </summary>
    [Fact]
    public async Task AddDatabasePersistence_ShouldRecoverFailedOutboxEntries()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using (var provider = BuildProvider(databasePath))
            {
                await provider.GetRequiredService<IDatabaseInitializer>()
                    .InitializeAsync(TestContext.Current.CancellationToken);
                await using var scope = provider.CreateAsyncScope();
                var store = scope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                var processorType = typeof(Messaging)
                    .Assembly
                    .GetTypes()
                    .Single(static type =>
                        type.Name ==
                        nameof(Raycynix.Extensions.Messaging.Implementations.MessageOutboxRecoveryProcessor));
                var processor = scope.ServiceProvider.GetRequiredService(processorType);

                await store.EnqueueAsync(CreateSerializedMessage("msg-3"), TestContext.Current.CancellationToken);
                await store.MarkFailedAsync(
                    "msg-3",
                    new InvalidOperationException("publish failed"),
                    DateTimeOffset.UtcNow.AddMinutes(-1),
                    TestContext.Current.CancellationToken);

                var processAvailableAsync = processor.GetType().GetMethod("ProcessAvailableAsync")!;
                var result =
                    await (Task<int>)processAvailableAsync.Invoke(processor, [TestContext.Current.CancellationToken])!;
                result.Should().Be(1);
            }

            await using var reloadedProvider = BuildProvider(databasePath);
            await using var reloadedScope = reloadedProvider.CreateAsyncScope();
            var storeAfterRecovery = reloadedScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
            var entryAfterRecovery = await storeAfterRecovery.GetAsync("msg-3", TestContext.Current.CancellationToken);

            entryAfterRecovery.Should().NotBeNull();
            entryAfterRecovery.Status.Should().Be(MessageOutboxStatus.Dispatched);
            entryAfterRecovery.AttemptCount.Should().Be(2);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that concurrent duplicate inbox inserts do not surface primary-key failures.
    /// </summary>
    [Fact]
    public async Task TryBeginProcessingAsync_WithConcurrentDuplicateDelivery_ShouldReturnFalseInsteadOfThrowing()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);
            var message = CreateIncomingMessage("msg-concurrent");
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var attempts = Enumerable.Range(0, 8)
                .Select(async _ =>
                {
                    await start.Task.ConfigureAwait(false);
                    await using var scope = provider.CreateAsyncScope();
                    var scopedStore = scope.ServiceProvider.GetRequiredService<IIncomingMessageInboxStore>();
                    return await scopedStore.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);
                })
                .ToArray();

            start.SetResult();
            var results = await Task.WhenAll(attempts);

            results.Count(static current => current).Should().Be(1);
            results.Count(static current => !current).Should().Be(7);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that stale Processing inbox entries can be reclaimed after an interrupted consumer run.
    /// </summary>
    [Fact]
    public async Task TryBeginProcessingAsync_WithStaleProcessingEntry_ShouldReclaimLease()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(
                databasePath,
                new Dictionary<string, string?>
                {
                    ["MessagingConfiguration:IncomingProcessing:ProcessingLeaseTimeout"] = "00:00:01"
                });
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);

            var message = CreateIncomingMessage("msg-stale");
            await using (var processingScope = provider.CreateAsyncScope())
            {
                var store = processingScope.ServiceProvider.GetRequiredService<IIncomingMessageInboxStore>();
                var firstAttempt = await store.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);
                firstAttempt.Should().BeTrue();
            }

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                var entry = await databaseContext.Set<MessagingInboxEntryEntity>()
                    .SingleAsync(current => current.MessageId == message.MessageId,
                        TestContext.Current.CancellationToken);
                entry.UpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using var reclaimScope = provider.CreateAsyncScope();
            var reclaimStore = reclaimScope.ServiceProvider.GetRequiredService<IIncomingMessageInboxStore>();
            var reclaimed = await reclaimStore.TryBeginProcessingAsync(message, TestContext.Current.CancellationToken);

            reclaimed.Should().BeTrue();
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that publishing within the ambient database unit of work does not flush unrelated business changes prematurely.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithAmbientBusinessChanges_ShouldDeferOutboxPersistenceUntilSaveChanges()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);

            var messageId = "msg-ambient-uow";

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                var envelopeFactory = scope.ServiceProvider.GetRequiredService<IMessageEnvelopeFactory>();
                var transportPublisher = provider.GetRequiredService<RecordingTransportPublisher>();

                databaseContext.Set<TestBusinessEntity>().Add(new TestBusinessEntity
                {
                    Id = "order-ambient",
                    Name = "pending"
                });

                var template = envelopeFactory.Create(
                    new PersistedInboxMessage("deferred"),
                    "orders.created",
                    MessageFormat.Json);
                var envelope = new MessageEnvelope<PersistedInboxMessage>
                {
                    Message = template.Message,
                    Destination = template.Destination,
                    Format = template.Format,
                    MessageId = messageId,
                    Contract = template.Contract,
                    CorrelationId = template.CorrelationId,
                    CausationId = template.CausationId,
                    CreatedAt = template.CreatedAt,
                    Headers = template.Headers
                };

                await publisher.PublishAsync(envelope, TestContext.Current.CancellationToken);
                transportPublisher.PublishedMessageIds.Should().BeEmpty();

                await using var beforeSaveScope = provider.CreateAsyncScope();
                var beforeSaveContext = beforeSaveScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                var beforeSaveStore = beforeSaveScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                (await beforeSaveContext.Set<TestBusinessEntity>().AnyAsync(TestContext.Current.CancellationToken))
                    .Should().BeFalse();
                (await beforeSaveStore.GetAsync(messageId, TestContext.Current.CancellationToken)).Should().BeNull();

                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using (var afterSaveScope = provider.CreateAsyncScope())
            {
                var afterSaveContext = afterSaveScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                var afterSaveStore = afterSaveScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                (await afterSaveContext.Set<TestBusinessEntity>().AnyAsync(entity => entity.Id == "order-ambient",
                        TestContext.Current.CancellationToken))
                    .Should()
                    .BeTrue();

                var entry = await afterSaveStore.GetAsync(messageId, TestContext.Current.CancellationToken);
                entry.Should().NotBeNull();
                entry.Status.Should().Be(MessageOutboxStatus.Pending);
            }

            await using (var recoveryScope = provider.CreateAsyncScope())
            {
                var recoveryProcessor = recoveryScope.ServiceProvider
                    .GetRequiredService<Raycynix.Extensions.Messaging.Implementations.MessageOutboxRecoveryProcessor>();
                var processed = await recoveryProcessor.ProcessAvailableAsync(TestContext.Current.CancellationToken);
                processed.Should().Be(1);
            }

            var recordingPublisher = provider.GetRequiredService<RecordingTransportPublisher>();
            recordingPublisher.PublishedMessageIds.Should().ContainSingle().Which.Should().Be(messageId);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that only one worker acquires concurrent outbox dispatch leases.
    /// </summary>
    [Fact]
    public async Task TryBeginDispatchAsync_WithConcurrentRecoveryWorkers_ShouldLeaseMessageOnlyOnce()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);
            await using (var seedScope = provider.CreateAsyncScope())
            {
                var store = seedScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                await store.EnqueueAsync(CreateSerializedMessage("msg-dispatch-race"),
                    TestContext.Current.CancellationToken);
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempts = Enumerable.Range(0, 8)
                .Select(async _ =>
                {
                    await start.Task.ConfigureAwait(false);
                    await using var scope = provider.CreateAsyncScope();
                    var scopedStore = scope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                    return await scopedStore.TryBeginDispatchAsync(
                        "msg-dispatch-race",
                        DateTimeOffset.UtcNow.AddMinutes(5),
                        TestContext.Current.CancellationToken);
                })
                .ToArray();

            start.SetResult();
            var results = await Task.WhenAll(attempts);

            results.Count(static current => current).Should().Be(1);
            results.Count(static current => !current).Should().Be(7);
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that stale outbox dispatch leases can be reclaimed after a crashed dispatcher run.
    /// </summary>
    [Fact]
    public async Task TryBeginDispatchAsync_WithStaleDispatchLease_ShouldReclaimMessage()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);
            await using (var seedScope = provider.CreateAsyncScope())
            {
                var store = seedScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
                await store.EnqueueAsync(CreateSerializedMessage("msg-dispatch-stale"),
                    TestContext.Current.CancellationToken);
                var leased = await store.TryBeginDispatchAsync(
                    "msg-dispatch-stale",
                    DateTimeOffset.UtcNow.AddMinutes(10),
                    TestContext.Current.CancellationToken);
                leased.Should().BeTrue();
            }

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                var entry = await databaseContext.Set<MessagingOutboxEntryEntity>()
                    .SingleAsync(current => current.MessageId == "msg-dispatch-stale",
                        TestContext.Current.CancellationToken);
                entry.NextAttemptAt = DateTimeOffset.UtcNow.AddMinutes(-10);
                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using var reclaimScope = provider.CreateAsyncScope();
            var reclaimStore = reclaimScope.ServiceProvider.GetRequiredService<IMessageOutboxStore>();
            var reclaimed = await reclaimStore.TryBeginDispatchAsync(
                "msg-dispatch-stale",
                DateTimeOffset.UtcNow.AddMinutes(5),
                TestContext.Current.CancellationToken);

            reclaimed.Should().BeTrue();
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that cleanup removes expired processed inbox rows.
    /// </summary>
    [Fact]
    public async Task CleanupProcessor_ShouldDeleteExpiredProcessedInboxRows()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                databaseContext.Set<MessagingInboxEntryEntity>().AddRange(
                    new MessagingInboxEntryEntity
                    {
                        MessageId = "inbox-expired",
                        Destination = "orders.processed",
                        Status = (int)IncomingMessageInboxStatus.Processed,
                        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-30)
                    },
                    new MessagingInboxEntryEntity
                    {
                        MessageId = "inbox-fresh",
                        Destination = "orders.processed",
                        Status = (int)IncomingMessageInboxStatus.Processed,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using (var cleanupScope = provider.CreateAsyncScope())
            {
                var processorType = typeof(MessagingDatabase)
                    .Assembly
                    .GetTypes()
                    .Single(static type => type.Name == nameof(MessagingDatabaseCleanupProcessor));
                var processor = cleanupScope.ServiceProvider.GetRequiredService(processorType);
                var processAsync = processorType.GetMethod(nameof(MessagingDatabaseCleanupProcessor.ProcessAsync))!;
                var deletedCount =
                    await (Task<int>)processAsync.Invoke(processor, [TestContext.Current.CancellationToken])!;
                deletedCount.Should().Be(1);
            }

            await using (var verificationScope = provider.CreateAsyncScope())
            {
                var databaseContext = verificationScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                (await databaseContext.Set<MessagingInboxEntryEntity>()
                        .AnyAsync(entry => entry.MessageId == "inbox-expired", TestContext.Current.CancellationToken))
                    .Should()
                    .BeFalse();
                (await databaseContext.Set<MessagingInboxEntryEntity>()
                        .AnyAsync(entry => entry.MessageId == "inbox-fresh", TestContext.Current.CancellationToken))
                    .Should()
                    .BeTrue();
            }
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    /// <summary>
    /// Verifies that cleanup removes expired dispatched outbox rows and keeps fresh ones.
    /// </summary>
    [Fact]
    public async Task CleanupProcessor_ShouldDeleteExpiredDispatchedOutboxRows()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        try
        {
            await using var provider = BuildProvider(databasePath);
            await provider.GetRequiredService<IDatabaseInitializer>()
                .InitializeAsync(TestContext.Current.CancellationToken);

            await using (var scope = provider.CreateAsyncScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                databaseContext.Set<MessagingOutboxEntryEntity>().AddRange(
                    new MessagingOutboxEntryEntity
                    {
                        MessageId = "outbox-expired",
                        Destination = "orders.created",
                        Payload = "{}"u8.ToArray(),
                        Format = (int)MessageFormat.Json,
                        ContentType = "application/json",
                        CreatedAt = DateTimeOffset.UtcNow.AddDays(-40),
                        Headers = "{}",
                        Status = (int)MessageOutboxStatus.Dispatched,
                        UpdatedAt = DateTimeOffset.UtcNow.AddDays(-30),
                        AttemptCount = 1,
                        NextAttemptAt = DateTimeOffset.MaxValue
                    },
                    new MessagingOutboxEntryEntity
                    {
                        MessageId = "outbox-fresh",
                        Destination = "orders.created",
                        Payload = "{}"u8.ToArray(),
                        Format = (int)MessageFormat.Json,
                        ContentType = "application/json",
                        CreatedAt = DateTimeOffset.UtcNow,
                        Headers = "{}",
                        Status = (int)MessageOutboxStatus.Dispatched,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        AttemptCount = 1,
                        NextAttemptAt = DateTimeOffset.MaxValue
                    });
                await databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            await using (var cleanupScope = provider.CreateAsyncScope())
            {
                var processorType = typeof(MessagingDatabase)
                    .Assembly
                    .GetTypes()
                    .Single(static type => type.Name == nameof(MessagingDatabaseCleanupProcessor));
                var processor = cleanupScope.ServiceProvider.GetRequiredService(processorType);
                var processAsync = processorType.GetMethod(nameof(MessagingDatabaseCleanupProcessor.ProcessAsync))!;
                var deletedCount =
                    await (Task<int>)processAsync.Invoke(processor, [TestContext.Current.CancellationToken])!;
                deletedCount.Should().Be(1);
            }

            await using (var verificationScope = provider.CreateAsyncScope())
            {
                var databaseContext = verificationScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
                (await databaseContext.Set<MessagingOutboxEntryEntity>()
                        .AnyAsync(entry => entry.MessageId == "outbox-expired", TestContext.Current.CancellationToken))
                    .Should()
                    .BeFalse();
                (await databaseContext.Set<MessagingOutboxEntryEntity>()
                        .AnyAsync(entry => entry.MessageId == "outbox-fresh", TestContext.Current.CancellationToken))
                    .Should()
                    .BeTrue();
            }
        }
        finally
        {
            TryDelete(databasePath);
        }
    }

    private static ServiceProvider BuildProvider(
        string databasePath,
        params IEnumerable<KeyValuePair<string, string?>>[] additionalConfiguration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton<InboxHandlerState>();
        services.AddSingleton<RecordingTransportPublisher>();
        services.AddSingleton<ITransportMessagePublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<RecordingTransportPublisher>());

        services.AddRaycynixDatabase(BuildDatabaseOptions(databasePath), registerCallerAssembly: false)
            .AddSqlite()
            .AddAssembly<MessagingDatabaseRegistrationTests>();
        services.AddRaycynixMessaging(BuildMessagingConfiguration(additionalConfiguration))
            .AddMessageHandler<PersistedInboxMessage, PersistedInboxHandler>()
            .AddDatabasePersistence();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static IConfiguration BuildDatabaseOptions(string databasePath)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = $"Data Source={databasePath}",
                ["DatabaseOptions:EnsureCreated"] = "true",
                ["DatabaseOptions:UseMigrations"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();
    }

    private static IConfiguration BuildMessagingConfiguration(
        params IEnumerable<KeyValuePair<string, string?>>[] additionalConfiguration)
    {
        var values = new Dictionary<string, string?>
        {
            ["MessagingConfiguration:Outbox:Enabled"] = "true",
            ["MessagingConfiguration:Outbox:EnableRecovery"] = "true",
            ["MessagingConfiguration:Outbox:AutoDispatchOnPublish"] = "false"
        };

        foreach (var pair in additionalConfiguration.SelectMany(static current => current))
        {
            values[pair.Key] = pair.Value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static SerializedMessage CreateSerializedMessage(string messageId)
    {
        return new SerializedMessage
        {
            MessageId = messageId,
            Destination = "orders.created",
            Payload = "{\"orderId\":\"order-1\"}"u8.ToArray(),
            Format = MessageFormat.Json,
            ContentType = "application/json",
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Format"] = nameof(MessageFormat.Json)
            }
        };
    }

    private static IncomingTransportMessage CreateIncomingMessage(string messageId)
    {
        return new IncomingTransportMessage
        {
            MessageId = messageId,
            Destination = "orders.persisted",
            Payload = "{\"value\":\"once\"}"u8.ToArray(),
            Format = MessageFormat.Json,
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Format"] = nameof(MessageFormat.Json)
            }
        };
    }

    private static void TryDelete(string databasePath)
    {
        try
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [MessageContract("orders.persisted", "1.0.0")]
    private sealed record PersistedInboxMessage(string Value);

    private sealed class InboxHandlerState
    {
        public List<string> Values { get; } = [];
    }

    /// <inheritdoc />
    private sealed class PersistedInboxHandler(InboxHandlerState state) : IMessageHandler<PersistedInboxMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<PersistedInboxMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.Values.Add(envelope.Message.Value);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingTransportPublisher : ITransportMessagePublisher
    {
        public List<string> PublishedMessageIds { get; } = [];

        public ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
        {
            PublishedMessageIds.Add(message.MessageId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingMessagingDatabaseInitializer
        : IDatabaseInitializer<RaycynixDatabaseContext>
    {
        public int CallCount { get; private set; }

        public bool IsReady => CallCount > 0;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingDatabaseInitializer : IDatabaseInitializer
    {
        public int CallCount { get; private set; }

        public bool IsReady => CallCount > 0;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    [DatabaseTable("messaging_database_test_business_entities")]
    private sealed class TestBusinessEntity
    {
        public string Id { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;
    }

    private sealed class TestBusinessEntityConfigurator : GenericConfigurator<TestBusinessEntity>
    {
        public override Type[] DependsOn => [];

        public override void Configure(ModelBuilder modelBuilder)
        {
            var entity = ConfigureEntity(modelBuilder);
            entity.HasKey(static current => current.Id);
            entity.Property(static current => current.Id).HasMaxLength(128);
            entity.Property(static current => current.Name).HasMaxLength(256);
        }
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        public void Log(LogLevel logLevel, Exception? exception, string message, params object?[]? args)
        {
        }
    }
}