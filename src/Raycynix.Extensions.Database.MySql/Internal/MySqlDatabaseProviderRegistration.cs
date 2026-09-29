using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using MySqlOptions = Raycynix.Extensions.Database.MySql.Options.MySqlOptions;

namespace Raycynix.Extensions.Database.MySql.Internal;

/// <summary>
/// Implements MySQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MySqlDatabaseProviderRegistration(
    MySqlOptions settings,
    ILogger<MySqlDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    private readonly MySqlOptions _settings = settings;

    /// <inheritdoc />
    public string ProviderName => "mysql";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw MySQL connection string");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new ArgumentException("Connection configuration is missing");

        var builder = new MySqlConnectionStringBuilder
        {
            Server = connection.Host,
            Port = (uint)(connection.Port ?? 3306),
            Database = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            AllowUserVariables = _settings.AllowUserVariables,
            Pooling = _settings.Pooling
        };

        logger?.LogDebug(
            "Resolved MySQL connection string from structured configuration. Pooling: {Pooling}, AllowUserVariables: {AllowUserVariables}, CommandTimeoutConfigured: {CommandTimeoutConfigured}",
            builder.Pooling,
            builder.AllowUserVariables,
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
        options.UseMySQL(connectionString, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            mySqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);
            if (configuration.MigrationsHistoryTable is not null)
                mySqlOptions.MigrationsHistoryTable(
                    configuration.MigrationsHistoryTable,
                    configuration.MigrationsHistorySchema
                );


            if (_settings.CommandTimeoutSeconds is not null)
                mySqlOptions.CommandTimeout(_settings.CommandTimeoutSeconds.Value);
        });

        logger?.LogDebug(
            "Configured EF Core MySQL provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}",
            migrationsAssembly.GetName().Name,
            configuration.RetryCount,
            configuration.RetryDelaySeconds);
    }

    /// <inheritdoc />
    public void Validate(DatabaseOptions configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Skipping structured MySQL validation because a raw connection string is configured");
            return;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new InvalidOperationException("MySQL connection configuration is missing");

        if (string.IsNullOrWhiteSpace(connection.Host))
            throw new InvalidOperationException("MySQL connection requires a host");

        if (string.IsNullOrWhiteSpace(connection.Name))
            throw new InvalidOperationException("MySQL connection requires a database name");

        if (string.IsNullOrWhiteSpace(connection.Username))
            throw new InvalidOperationException("MySQL connection requires a username");

        logger?.LogDebug("MySQL structured connection configuration validated");
    }
}