using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Sqlite.Options;

namespace Raycynix.Extensions.Database.Sqlite.Internal;

/// <summary>
/// Implements SQLite-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class SqliteDatabaseProviderRegistration(
    SqliteOptions settings,
    ILogger<SqliteDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "sqlite";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw SQLite connection string");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = connection.Name
        };

        if (!string.IsNullOrWhiteSpace(settings.Mode))
            builder.Mode = Enum.Parse<SqliteOpenMode>(settings.Mode, ignoreCase: true);

        if (!string.IsNullOrWhiteSpace(settings.Cache))
            builder.Cache = Enum.Parse<SqliteCacheMode>(settings.Cache, ignoreCase: true);


        logger?.LogDebug(
            "Resolved SQLite connection string from structured configuration. ModeConfigured: {ModeConfigured}, CacheConfigured: {CacheConfigured}, CommandTimeoutConfigured: {CommandTimeoutConfigured}",
            !string.IsNullOrWhiteSpace(settings.Mode),
            !string.IsNullOrWhiteSpace(settings.Cache),
            settings.CommandTimeoutSeconds is not null);

        return builder.ToString();
    }

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseOptions configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);
            if (configuration.MigrationsHistoryTable is not null)
                sqliteOptions.MigrationsHistoryTable(
                    configuration.MigrationsHistoryTable,
                    configuration.MigrationsHistorySchema);


            if (settings.CommandTimeoutSeconds is not null)
                sqliteOptions.CommandTimeout(settings.CommandTimeoutSeconds.Value);
        });

        logger?.LogDebug(
            "Configured EF Core SQLite provider. Migrations assembly: {MigrationsAssembly}",
            migrationsAssembly.GetName().Name);
    }

    /// <inheritdoc />
    public void Validate(DatabaseOptions configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Skipping structured SQLite validation because a raw connection string is configured");
            return;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new InvalidOperationException("SQLite connection configuration is missing");

        if (string.IsNullOrWhiteSpace(connection.Name))
            throw new InvalidOperationException("SQLite connection requires a data source name");


        logger?.LogDebug("SQLite structured connection configuration validated");
    }
}