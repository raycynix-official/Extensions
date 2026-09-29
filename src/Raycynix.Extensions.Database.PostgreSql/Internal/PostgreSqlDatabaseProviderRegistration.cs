using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.PostgreSql.Options;

namespace Raycynix.Extensions.Database.PostgreSql.Internal;

/// <summary>
/// Implements PostgreSQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class PostgreSqlDatabaseProviderRegistration(
    PostgreSqlOptions settings,
    ILogger<PostgreSqlDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    private readonly PostgreSqlOptions _settings = settings;

    /// <inheritdoc />
    public string ProviderName => "postgresql";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw PostgreSQL connection string");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new ArgumentException("Connection configuration is missing");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Host,
            Port = connection.Port ?? 5432,
            Database = connection.Name,
            Username = connection.Username,
            Password = connection.Password,
            Pooling = _settings.Pooling,
            IncludeErrorDetail = _settings.IncludeErrorDetail
        };

        if (_settings.MinimumPoolSize is not null)
            builder.MinPoolSize = _settings.MinimumPoolSize.Value;


        if (_settings.MaximumPoolSize is not null)
            builder.MaxPoolSize = _settings.MaximumPoolSize.Value;


        if (_settings.CommandTimeoutSeconds is not null)
            builder.CommandTimeout = _settings.CommandTimeoutSeconds.Value;


        logger?.LogDebug(
            "Resolved PostgreSQL connection string from structured configuration. Pooling: {Pooling}, MinimumPoolSizeConfigured: {MinimumPoolSizeConfigured}, MaximumPoolSizeConfigured: {MaximumPoolSizeConfigured}, CommandTimeoutConfigured: {CommandTimeoutConfigured}",
            builder.Pooling,
            _settings.MinimumPoolSize is not null,
            _settings.MaximumPoolSize is not null,
            _settings.CommandTimeoutSeconds is not null);

        return builder.ConnectionString;
    }

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseOptions configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            npgsqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);
            if (configuration.MigrationsHistoryTable is not null)
                npgsqlOptions.MigrationsHistoryTable(
                    configuration.MigrationsHistoryTable,
                    configuration.MigrationsHistorySchema);


            if (_settings.CommandTimeoutSeconds is not null)
                npgsqlOptions.CommandTimeout(_settings.CommandTimeoutSeconds.Value);
        });

        logger?.LogDebug(
            "Configured EF Core PostgreSQL provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}",
            migrationsAssembly.GetName().Name,
            configuration.RetryCount,
            configuration.RetryDelaySeconds);
    }

    /// <inheritdoc />
    public void Validate(DatabaseOptions configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug(
                "Skipping structured PostgreSQL validation because a raw connection string is configured");
            return;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new InvalidOperationException("PostgreSQL connection configuration is missing");

        if (string.IsNullOrWhiteSpace(connection.Host))
            throw new InvalidOperationException("PostgreSQL connection requires a host");


        if (string.IsNullOrWhiteSpace(connection.Name))
            throw new InvalidOperationException("PostgreSQL connection requires a database name");


        if (string.IsNullOrWhiteSpace(connection.Username))
            throw new InvalidOperationException("PostgreSQL connection requires a username");


        logger?.LogDebug("PostgreSQL structured connection configuration validated");
    }
}