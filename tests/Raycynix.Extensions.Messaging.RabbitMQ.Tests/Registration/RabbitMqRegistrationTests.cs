using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;
using Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;
using Raycynix.Extensions.Messaging.RabbitMQ.Internal;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Tests.Registration;

/// <summary>
/// Covers service registration, publishing, and inbound processing behavior for the Raycynix RabbitMQ messaging package.
/// </summary>
public sealed class RabbitMqRegistrationTests
{
    /// <summary>
    /// Verifies that RabbitMQ transport registration registers transport options and the RabbitMQ transport publisher.
    /// </summary>
    [Fact]
    public async Task AddRabbitMq_ShouldRegisterRabbitMqConfigurationAndPublisher()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRabbitMq(options =>
            {
                options.Host = "rabbit";
                options.Queue.Name = "messages";
            });

        await using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<RabbitMqMessagingConfiguration>().Queue.Name.Should().Be("messages");
        provider.GetRequiredService<IMessagePublisher>().Should().NotBeNull();
        provider.GetRequiredService<ITransportMessagePublisher>().GetType().Name.Should().Be("RabbitMqMessagePublisher");
    }

    /// <summary>
    /// Verifies that RabbitMQ transport options can be bound from configuration using the default section name.
    /// </summary>
    [Fact]
    public async Task AddRabbitMq_WithConfiguration_ShouldBindConfigurationAndRegisterPublisher()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RabbitMqMessagingConfiguration:Host"] = "rabbit.internal",
                ["RabbitMqMessagingConfiguration:Port"] = "5673",
                ["RabbitMqMessagingConfiguration:Exchange:Name"] = "integration.events",
                ["RabbitMqMessagingConfiguration:Queue:Name"] = "orders.created"
            })
            .Build();

        services.AddRaycynixMessaging(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build())
            .AddRabbitMq(configuration);

        await using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<RabbitMqMessagingConfiguration>();
        options.Host.Should().Be("rabbit.internal");
        options.Port.Should().Be(5673);
        options.Exchange.Name.Should().Be("integration.events");
        options.Queue.Name.Should().Be("orders.created");
        provider.GetRequiredService<ITransportMessagePublisher>().GetType().Name.Should().Be("RabbitMqMessagePublisher");
    }

    /// <summary>
    /// Verifies that RabbitMQ publishing initializes topology and publishes to the configured exchange.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldInitializeTopologyAndPublishToConfiguredExchange()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();

        services.AddRaycynixMessaging(configuration)
            .AddRabbitMq(options =>
            {
                options.Host = "localhost";
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
            });

        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelope = envelopeFactory.Create(new TestMessage("hello"), "orders.created", MessageFormat.Json);

        await publisher.PublishAsync(envelope, TestContext.Current.CancellationToken);

        fakeConnectionFactory.Connection.Should().NotBeNull();
        fakeConnectionFactory.Connection!.PublishedExchange.Should().Be("integration.events");
        fakeConnectionFactory.Connection.PublishedRoutingKey.Should().Be("orders.created");
        fakeConnectionFactory.Connection.TopologyExchangeDeclared.Should().BeTrue();
        fakeConnectionFactory.Connection.TopologyQueueDeclared.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the RabbitMQ inbound consumer dispatches incoming messages to registered handlers.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_ShouldDispatchIncomingMessageToRegisteredHandler()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();
        var processingState = new ProcessingState();

        services.AddSingleton(processingState);
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<InboundMessage, RecordingInboundHandler>()
            .AddRabbitMq(options =>
            {
                options.Host = "localhost";
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
                options.Consumer.Enabled = true;
                options.Consumer.PollIntervalMilliseconds = 10;
            });

        fakeConnectionFactory.Enqueue(CreateDelivery("orders.created", """{"value":"hello"}"""));
        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new RabbitMqInboundConsumer(
            provider.GetRequiredService<RabbitMqConnectionAccessor>(),
            provider.GetRequiredService<RabbitMqMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await processingState.Processed.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await WaitForAsync(
            () => fakeConnectionFactory.Connection!.AckedDeliveryTags.Contains(1),
            TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        processingState.Values.Should().ContainSingle().Which.Should().Be("hello");
        fakeConnectionFactory.Connection!.AckedDeliveryTags.Should().Contain(1);
    }

    /// <summary>
    /// Verifies that inbound failures are republished for retry before the original message is acknowledged.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_WhenHandlerFails_ShouldRepublishForRetry()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();

        services.AddSingleton<AlwaysFailingState>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = false;
            })
            .AddMessageHandler<InboundMessage, AlwaysFailingInboundHandler>()
            .AddRabbitMq(options =>
            {
                options.Host = "localhost";
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
                options.Consumer.Enabled = true;
                options.Consumer.PollIntervalMilliseconds = 10;
                options.Retry.Enabled = true;
                options.Retry.MaxAttempts = 2;
                options.Retry.DelayMilliseconds = 0;
            });

        fakeConnectionFactory.Enqueue(CreateDelivery("orders.created", """{"value":"retry-me"}"""));
        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new RabbitMqInboundConsumer(
            provider.GetRequiredService<RabbitMqConnectionAccessor>(),
            provider.GetRequiredService<RabbitMqMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await fakeConnectionFactory.Connection!.Acknowledged.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        fakeConnectionFactory.Connection!.PublishedExchange.Should().Be("integration.events");
        fakeConnectionFactory.Connection.PublishedRoutingKey.Should().Be("orders.created");
        fakeConnectionFactory.Connection.PublishedHeaders.Should().ContainKey("X-Delivery-Attempt");
        System.Text.Encoding.UTF8.GetString(
                (byte[])fakeConnectionFactory.Connection.PublishedHeaders["X-Processing-Error"]!)
            .Should().Be(nameof(InvalidOperationException));
        fakeConnectionFactory.Connection.AckedDeliveryTags.Should().Contain(1);
    }

    /// <summary>
    /// Verifies that stopping the host cancels an in-progress RabbitMQ retry delay.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_DuringRetryDelay_ShouldStopPromptly()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();

        services.AddSingleton<AlwaysFailingState>();
        services.AddRaycynixMessaging(configuration, options => options.DispatchRetry.Enabled = false)
            .AddMessageHandler<InboundMessage, AlwaysFailingInboundHandler>()
            .AddRabbitMq(options =>
            {
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
                options.Consumer.Enabled = true;
                options.Retry.Enabled = true;
                options.Retry.MaxAttempts = 2;
                options.Retry.DelayMilliseconds = 30_000;
            });

        fakeConnectionFactory.Enqueue(CreateDelivery("orders.created", """{"value":"retry-me"}"""));
        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new RabbitMqInboundConsumer(
            provider.GetRequiredService<RabbitMqConnectionAccessor>(),
            provider.GetRequiredService<RabbitMqMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => fakeConnectionFactory.Connection?.Deliveries.Count == 0, TimeSpan.FromSeconds(2),
            TestContext.Current.CancellationToken);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await hostedService.StopAsync(stopTimeout.Token);

        fakeConnectionFactory.Connection!.PublishedExchange.Should().BeNull();
        fakeConnectionFactory.Connection.AckedDeliveryTags.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that inbound failures are dead-lettered when retries are exhausted.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_WhenRetriesAreExhausted_ShouldPublishToDeadLetterExchange()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();

        services.AddSingleton<AlwaysFailingState>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = false;
            })
            .AddMessageHandler<InboundMessage, AlwaysFailingInboundHandler>()
            .AddRabbitMq(options =>
            {
                options.Host = "localhost";
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
                options.Consumer.Enabled = true;
                options.Consumer.PollIntervalMilliseconds = 10;
                options.Retry.Enabled = true;
                options.Retry.MaxAttempts = 1;
                options.Retry.DelayMilliseconds = 0;
                options.DeadLetter.Enabled = true;
                options.DeadLetter.Exchange = "integration.dlx";
                options.DeadLetter.RoutingKey = "dead-letter";
            });

        fakeConnectionFactory.Enqueue(CreateDelivery(
            "orders.created",
            """{"value":"dead-letter-me"}""",
            headers: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Delivery-Attempt"] = "1"
            }));
        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new RabbitMqInboundConsumer(
            provider.GetRequiredService<RabbitMqConnectionAccessor>(),
            provider.GetRequiredService<RabbitMqMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await fakeConnectionFactory.Connection!.Acknowledged.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        fakeConnectionFactory.Connection!.PublishedExchange.Should().Be("integration.dlx");
        fakeConnectionFactory.Connection.PublishedRoutingKey.Should().Be("dead-letter");
        fakeConnectionFactory.Connection.AckedDeliveryTags.Should().Contain(1);
    }

    private sealed record TestMessage(string Value);

    [MessageContract("orders.created", "1.0.0")]
    private sealed record InboundMessage(string Value);

    private sealed class ProcessingState
    {
        public List<string> Values { get; } = [];

        public TaskCompletionSource Processed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class AlwaysFailingState;

    private sealed class RecordingInboundHandler(ProcessingState state) : IMessageHandler<InboundMessage>
    {
        public ValueTask HandleAsync(MessageEnvelope<InboundMessage> envelope, CancellationToken cancellationToken = default)
        {
            state.Values.Add(envelope.Message.Value);
            state.Processed.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AlwaysFailingInboundHandler : IMessageHandler<InboundMessage>
    {
        public ValueTask HandleAsync(MessageEnvelope<InboundMessage> envelope, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("handler failed");
        }
    }

    private sealed class FakeRabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        public FakeRabbitMqConnection? Connection { get; private set; }

        public void Enqueue(RabbitMqIncomingDelivery delivery)
        {
            Connection ??= new FakeRabbitMqConnection();
            Connection.Deliveries.Enqueue(delivery);
        }

        public Task<IRabbitMqConnection> CreateAsync(
            RabbitMqMessagingConfiguration configuration,
            CancellationToken cancellationToken)
        {
            Connection ??= new FakeRabbitMqConnection();
            return Task.FromResult<IRabbitMqConnection>(Connection);
        }
    }

    private sealed class FakeRabbitMqConnection : IRabbitMqConnection
    {
        public bool TopologyExchangeDeclared { get; set; }

        public bool TopologyQueueDeclared { get; set; }

        public string? PublishedExchange { get; set; }

        public string? PublishedRoutingKey { get; set; }

        public Dictionary<string, object?> PublishedHeaders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public Queue<RabbitMqIncomingDelivery> Deliveries { get; } = new();

        public List<ulong> AckedDeliveryTags { get; } = [];

        public TaskCompletionSource Acknowledged { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IRabbitMqChannel>(new FakeRabbitMqChannel(this));
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeRabbitMqChannel(FakeRabbitMqConnection connection) : IRabbitMqChannel
    {
        public Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<RabbitMqIncomingDelivery?> BasicGetAsync(string queue, bool autoAck, CancellationToken cancellationToken)
        {
            return Task.FromResult(connection.Deliveries.Count > 0 ? connection.Deliveries.Dequeue() : null);
        }

        public Task ExchangeDeclareAsync(
            string exchange,
            string type,
            bool durable,
            bool autoDelete,
            IDictionary<string, object?>? arguments,
            bool passive,
            bool noWait,
            CancellationToken cancellationToken)
        {
            connection.TopologyExchangeDeclared = true;
            return Task.CompletedTask;
        }

        public Task QueueDeclareAsync(
            string queue,
            bool durable,
            bool exclusive,
            bool autoDelete,
            IDictionary<string, object?>? arguments,
            bool passive,
            bool noWait,
            CancellationToken cancellationToken)
        {
            connection.TopologyQueueDeclared = true;
            return Task.CompletedTask;
        }

        public Task QueueBindAsync(
            string queue,
            string exchange,
            string routingKey,
            IDictionary<string, object?>? arguments,
            bool noWait,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task BasicPublishAsync(
            string exchange,
            string routingKey,
            bool mandatory,
            BasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken)
        {
            connection.PublishedExchange = exchange;
            connection.PublishedRoutingKey = routingKey;
            connection.PublishedHeaders = basicProperties.Headers?.ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            return Task.CompletedTask;
        }

        public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple, CancellationToken cancellationToken)
        {
            connection.AckedDeliveryTags.Add(deliveryTag);
            connection.Acknowledged.TrySetResult();
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask BasicRejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken)
        {
            return new ValueTask(Task.CompletedTask);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private static RabbitMqIncomingDelivery CreateDelivery(
        string routingKey,
        string json,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var actualHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Contract-Name"] = "orders.created",
            ["X-Contract-Version"] = "1.0.0",
            ["X-Message-Format"] = nameof(MessageFormat.Json)
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                actualHeaders[header.Key] = header.Value;
            }
        }

        return new RabbitMqIncomingDelivery
        {
            DeliveryTag = 1,
            RoutingKey = routingKey,
            Body = System.Text.Encoding.UTF8.GetBytes(json),
            MessageId = Guid.NewGuid().ToString("N"),
            CorrelationId = "corr-1",
            ContentType = "application/json",
            Timestamp = DateTimeOffset.UtcNow,
            Headers = actualHeaders
        };
    }

    private static async Task WaitForAsync(
        Func<bool> condition,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (!condition())
        {
            if (DateTimeOffset.UtcNow - startedAt >= timeout)
            {
                throw new TimeoutException("The expected RabbitMQ test condition was not met in time.");
            }

            await Task.Delay(25, cancellationToken);
        }
    }
}
