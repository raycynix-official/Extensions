namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Represents an EF Core database context that participates in Raycynix database model caching.
/// </summary>
public interface IRaycynixDatabaseContext
{
    /// <summary>
    /// Gets the cache key fragment that identifies the current Raycynix model shape.
    /// </summary>
    /// <returns>The model cache key fragment for the current context instance.</returns>
    string GetModelCacheKey();
}