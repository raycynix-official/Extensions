using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Applies registered database model configurators and provides a model cache discriminator.
/// </summary>
public interface IDatabaseModelConfigurator
{
    /// <summary>
    /// Applies model configuration for the active database provider.
    /// </summary>
    /// <param name="modelBuilder">The EF Core model builder to configure.</param>
    /// <param name="providerName">The normalized logical name of the active provider.</param>
    void Configure(ModelBuilder modelBuilder, string providerName);

    /// <summary>
    /// Builds the cache key fragment for the configured model shape.
    /// </summary>
    /// <param name="providerName">The normalized logical name of the active provider.</param>
    /// <returns>The model cache key fragment for the active provider and registered configurators.</returns>
    string GetModelCacheKey(string providerName);
}