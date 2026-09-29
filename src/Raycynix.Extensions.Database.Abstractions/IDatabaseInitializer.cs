namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines the contract for preparing the database during application startup.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database according to the configured strategy.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the initialization operation.</param>
    /// <returns>A task that completes when initialization finishes.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value indicating whether the database is ready for use.
    /// </summary>
    bool IsReady { get; }
}

/// <summary>
/// Defines database initialization for one concrete context type.
/// </summary>
/// <typeparam name="TContext">The context to initialize.</typeparam>
public interface IDatabaseInitializer<TContext> : IDatabaseInitializer
    where TContext : Microsoft.EntityFrameworkCore.DbContext, IRaycynixDatabaseContext;