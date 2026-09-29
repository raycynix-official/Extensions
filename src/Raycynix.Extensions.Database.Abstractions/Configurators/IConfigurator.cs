using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions.Configurators;

/// <summary>
/// Defines the contract for entity model configuration and seeding.
/// </summary>
public interface IConfigurator
{
    /// <summary>
    /// Gets the entity type handled by the configurator.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the entity types whose configurators must run before the current one.
    /// </summary>
    Type[] DependsOn { get; }

    /// <summary>
    /// Gets the cache key fragment that identifies the model shape produced by the configurator.
    /// </summary>
    string ModelCacheKey { get; }

    /// <summary>
    /// Applies model configuration for the entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entity.</param>
    void Configure(ModelBuilder modelBuilder);

    /// <summary>
    /// Registers seed data for the entity.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to register seed data.</param>
    void Seed(ModelBuilder modelBuilder);
}