using Microsoft.EntityFrameworkCore;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Applies provider-specific model defaults before entity configurators.
/// </summary>
public interface IDatabaseProviderModelConfigurator
{
    /// <summary>Gets the provider to which these defaults apply.</summary>
    string ProviderName { get; }

    /// <summary>Gets a stable key identifying all model-affecting settings.</summary>
    string ModelCacheKey { get; }

    /// <summary>Applies provider defaults to the model.</summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    void Configure(ModelBuilder modelBuilder);
}
