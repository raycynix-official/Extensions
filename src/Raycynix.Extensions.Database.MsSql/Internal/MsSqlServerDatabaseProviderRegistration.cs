using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MsSql.Options;

namespace Raycynix.Extensions.Database.MsSql.Internal;

/// <summary>
/// Implements SQL Server-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MsSqlServerDatabaseProviderRegistration(
    MsSqlServerOptions settings,
    ILogger<MsSqlServerDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    private readonly MsSqlServerOptions _settings = settings;

    /// <inheritdoc />
    public string ProviderName => "sqlserver";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw SQL Server connection string");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new ArgumentException("Connection configuration is missing");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Host,
            InitialCatalog = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            TrustServerCertificate = _settings.TrustServerCertificate,
            MultipleActiveResultSets = _settings.MultipleActiveResultSets
        };

        logger?.LogDebug(
            "Resolved SQL Server connection string from structured configuration. TrustServerCertificate: {TrustServerCertificate}, MultipleActiveResultSets: {MultipleActiveResultSets}, CommandTimeoutConfigured: {CommandTimeoutConfigured}",
            builder.TrustServerCertificate,
            builder.MultipleActiveResultSets,
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
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            sqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);
            if (configuration.MigrationsHistoryTable is not null)
            {
                sqlOptions.MigrationsHistoryTable(
                    configuration.MigrationsHistoryTable,
                    configuration.MigrationsHistorySchema);
            }

            if (_settings.CommandTimeoutSeconds is not null)
            {
                sqlOptions.CommandTimeout(_settings.CommandTimeoutSeconds.Value);
            }
        });

        logger?.LogDebug(
            "Configured EF Core SQL Server provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}",
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
                "Skipping structured SQL Server validation because a raw connection string is configured");
            return;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new InvalidOperationException("SQL Server connection configuration is missing");

        if (string.IsNullOrWhiteSpace(connection.Host))
        {
            throw new InvalidOperationException("SQL Server connection requires a host");
        }

        if (string.IsNullOrWhiteSpace(connection.Name))
        {
            throw new InvalidOperationException("SQL Server connection requires a database name");
        }

        logger?.LogDebug("SQL Server structured connection configuration validated");
    }
}