using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MySql;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Tests.Registration;

/// <summary>
/// Verifies provider-agnostic model metadata required by the database-backed messaging stores.
/// </summary>
public sealed class MessagingDatabaseProviderModelTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(false, "messaging")]
    [InlineData(true, null)]
    [InlineData(true, "messaging")]
    public void InboxAndOutbox_ShouldInheritProviderDefaultSchema(bool sqlServer, string? schema)
    {
        using var provider = BuildProvider(
            sqlServer ? "Server=localhost;Database=messaging;Trusted_Connection=true" : "Host=localhost;Database=messaging",
            builder => sqlServer
                ? builder.AddMsSql(options => options.DefaultSchema = schema)
                : builder.AddPostgreSql(options => options.DefaultSchema = schema));
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        foreach (var type in new[] { typeof(MessagingInboxEntryEntity), typeof(MessagingOutboxEntryEntity) })
        {
            var entity = context.Model.FindEntityType(type)!;
            entity.GetSchema().Should().Be(schema);
            var table = sqlServer ? $"[{entity.GetTableName()}]" : entity.GetTableName();
            var qualifiedTable = schema is null ? table : sqlServer ? $"[{schema}].{table}" : $"{schema}.{table}";
            context.Database.GenerateCreateScript().Should().Contain($"CREATE TABLE {qualifiedTable}");
        }
    }

    /// <summary>
    /// Verifies that the SQLite provider builds the messaging model with concurrency tokens required for optimistic leases.
    /// </summary>
    [Fact]
    public void MessagingPersistenceModel_ShouldConfigureConcurrencyTokens_ForSqlite()
    {
        using var provider = BuildProvider(
            "Data Source=model-sqlite.db",
            static builder => builder.AddSqlite());

        AssertConcurrencyModel(provider);
    }

    /// <summary>
    /// Verifies that the PostgreSQL provider builds the messaging model with concurrency tokens required for optimistic leases.
    /// </summary>
    [Fact]
    public void MessagingPersistenceModel_ShouldConfigureConcurrencyTokens_ForPostgreSql()
    {
        using var provider = BuildProvider(
            "Host=localhost;Port=5432;Database=messaging;Username=test;Password=test",
            static builder => builder.AddPostgreSql());

        AssertConcurrencyModel(provider);
    }

    /// <summary>
    /// Verifies that the SQL Server provider builds the messaging model with concurrency tokens required for optimistic leases.
    /// </summary>
    [Fact]
    public void MessagingPersistenceModel_ShouldConfigureConcurrencyTokens_ForMsSql()
    {
        using var provider = BuildProvider(
            "Server=localhost;Database=messaging;User Id=sa;Password=Password123!;TrustServerCertificate=true",
            static builder => builder.AddMsSql());

        AssertConcurrencyModel(provider);
    }

    /// <summary>
    /// Verifies that the MySQL provider builds the messaging model with concurrency tokens required for optimistic leases.
    /// </summary>
    [Fact]
    public void MessagingPersistenceModel_ShouldConfigureConcurrencyTokens_ForMySql()
    {
        using var provider = BuildProvider(
            "Server=localhost;Port=3306;Database=messaging;User ID=test;Password=test",
            static builder => builder.AddMySql());

        AssertConcurrencyModel(provider);
    }

    /// <summary>
    /// Asserts that the messaging inbox and outbox entities are present in the model and use optimistic concurrency.
    /// </summary>
    /// <param name="provider">The root service provider that resolves the shared database context.</param>
    private static void AssertConcurrencyModel(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        var inbox = context.Model.FindEntityType(typeof(MessagingInboxEntryEntity));
        var outbox = context.Model.FindEntityType(typeof(MessagingOutboxEntryEntity));

        inbox.Should().NotBeNull();
        outbox.Should().NotBeNull();
        inbox.FindProperty(nameof(MessagingInboxEntryEntity.UpdatedAt))!.IsConcurrencyToken.Should().BeTrue();
        outbox.FindProperty(nameof(MessagingOutboxEntryEntity.UpdatedAt))!.IsConcurrencyToken.Should().BeTrue();
        inbox.GetTableName().Should().NotBeNullOrWhiteSpace();
        outbox.GetTableName().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Builds a database service provider for a specific relational provider without auto-registering the test assembly.
    /// </summary>
    /// <param name="connectionString">The provider-specific connection string used to configure the shared database context.</param>
    /// <param name="registerProvider">The callback that attaches the desired provider package to the database builder.</param>
    /// <returns>A fully built service provider for model inspection.</returns>
    private static ServiceProvider BuildProvider(
        string connectionString,
        Func<IDatabaseBuilder, IDatabaseBuilder> registerProvider)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        services.AddSingleton(new MessagingDatabasePersistenceConfiguration());

        var builder = services.AddRaycynixDatabase(
            BuildDatabaseOptions(connectionString),
            registerCallerAssembly: false);
        registerProvider(builder);
        services.AddRaycynixDatabaseAssembly(typeof(MessagingInboxEntryEntity).Assembly);

        return services.BuildServiceProvider(validateScopes: true);
    }

    /// <summary>
    /// Creates the minimal database configuration required to build the messaging model.
    /// </summary>
    /// <param name="connectionString">The provider-specific connection string.</param>
    /// <returns>An in-memory configuration source for the database package.</returns>
    private static IConfiguration BuildDatabaseOptions(string connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = connectionString,
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();
    }

    /// <summary>
    /// Provides a no-op logger for registration tests that only inspect metadata.
    /// </summary>
    /// <typeparam name="T">The log category type.</typeparam>
    private sealed class FakeLogger<T> : ILogger<T>
    {
        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        /// <inheritdoc />
        public void Log(LogLevel logLevel, Exception? exception, string message, params object?[]? args)
        {
        }
    }
}
