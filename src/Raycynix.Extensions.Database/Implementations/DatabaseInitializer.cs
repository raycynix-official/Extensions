using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Initializes the database by creating it and/or applying migrations, depending on the configuration.
/// </summary>
public class DatabaseInitializer<TContext>(
    IServiceScopeFactory serviceScopeFactory,
    IDatabaseObservability observability,
    DatabaseOptions config,
    DatabaseProviderDescriptor descriptor,
    ILogger<DatabaseInitializer<TContext>>? logger = null) : IDatabaseInitializer<TContext>
    where TContext : DbContext, IRaycynixDatabaseContext
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly string _providerName = descriptor.ProviderName;

    /// <summary>
    /// Gets a value indicating whether initialization has already completed.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Runs the configured database initialization steps at once.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsReady)
        {
            logger?.LogDebug("Database is already initialized for provider {ProviderName}", _providerName);
            return;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (IsReady)
            {
                logger?.LogDebug("Database is already initialized for provider {ProviderName}", _providerName);
                return;
            }

            logger?.LogInformation("Starting database initialization for provider {ProviderName}", _providerName);
            using var initializationScope = observability.BeginOperation(_providerName, "initialization");

            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            if (config.EnsureCreated)
            {
                logger?.LogInformation("Applying database creation for provider {ProviderName}", _providerName);
                using var ensureCreatedScope = observability.BeginOperation(_providerName, "ensure_created");

                try
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    observability.RecordSuccess(_providerName, "ensure_created");
                    logger?.LogDebug("Database creation completed for provider {ProviderName}", _providerName);
                }
                catch (Exception exception)
                {
                    observability.RecordFailure(_providerName, "ensure_created");
                    logger?.LogError(
                        exception,
                        "Database creation failed for provider {ProviderName}",
                        _providerName);
                    throw;
                }
            }

            if (config.UseMigrations)
            {
                logger?.LogInformation("Applying database migrations for provider {ProviderName}", _providerName);
                using var migrationsScope = observability.BeginOperation(_providerName, "migrate");

                try
                {
                    await context.Database.MigrateAsync(cancellationToken);
                    observability.RecordSuccess(_providerName, "migrate");
                    logger?.LogDebug("Database migrations completed for provider {ProviderName}", _providerName);
                }
                catch (Exception exception)
                {
                    observability.RecordFailure(_providerName, "migrate");
                    logger?.LogError(
                        exception,
                        "Database migration failed for provider {ProviderName}",
                        _providerName);
                    throw;
                }
            }

            IsReady = true;
            observability.RecordSuccess(_providerName, "initialization");
            logger?.LogInformation("Database initialization completed for provider {ProviderName}", _providerName);
        }
        catch (Exception exception)
        {
            observability.RecordFailure(_providerName, "initialization");
            logger?.LogError(
                exception,
                "Database initialization failed for provider {ProviderName}",
                _providerName);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}