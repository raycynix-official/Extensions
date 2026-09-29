using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Exposes the database infrastructure isolated for a concrete context type.
/// </summary>
/// <typeparam name="TContext">The context that owns the services.</typeparam>
public interface IDatabaseContextServices<TContext>
    where TContext : DbContext, IRaycynixDatabaseContext
{
    /// <summary>
    /// Gets the options bound for this context.
    /// </summary>
    DatabaseOptions Options { get; }

    /// <summary>
    /// Gets the provider selected for this context.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Applies this context's model configurators.
    /// </summary>
    void ConfigureModel(ModelBuilder modelBuilder);

    /// <summary>
    /// Gets the model cache key for this context.
    /// </summary>
    string GetModelCacheKey();
}