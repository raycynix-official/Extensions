using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Applies registered EF Core configurators and optional seed data to a Raycynix database model.
/// </summary>
/// <param name="config">The active database configuration.</param>
/// <param name="modelAssemblyRegistry">The registry of assemblies that contribute model configurators.</param>
/// <param name="observability">The observability implementation used to record model-building operations.</param>
/// <param name="serviceProvider">The service provider used to activate configurators.</param>
/// <param name="logger">The optional logger used to record model configuration diagnostics.</param>
public sealed class DatabaseModelConfigurator(
    DatabaseOptions config,
    IDatabaseModelAssemblyRegistry modelAssemblyRegistry,
    IDatabaseObservability observability,
    IServiceProvider serviceProvider,
    ILogger<DatabaseModelConfigurator>? logger = null)
    : IDatabaseModelConfigurator
{
    /// <inheritdoc />
    public void Configure(ModelBuilder modelBuilder, string providerName)
    {
        try
        {
            using var modelCreatingScope = observability.BeginOperation(providerName, "model_creating");

            var configurators = GetConfigurators();
            foreach (var providerConfigurator in GetProviderConfigurators(providerName))
            {
                providerConfigurator.Configure(modelBuilder);
            }
            observability.AddTag("database.configurator.count", configurators.Count.ToString());
            logger?.LogDebug(
                "Configuring database model for provider {ProviderName}. Configurator count: {ConfiguratorCount}. Seed enabled: {SeedEnabled}.",
                providerName,
                configurators.Count,
                config.EnableSeed);

            foreach (var configurator in configurators)
            {
                logger?.LogDebug(
                    "Applying database model configurator {ConfiguratorType} for provider {ProviderName}.",
                    configurator.GetType().Name,
                    providerName);

                configurator.Configure(modelBuilder);

                if (config.EnableSeed)
                {
                    logger?.LogDebug(
                        "Applying database seed from configurator {ConfiguratorType} for provider {ProviderName}.",
                        configurator.GetType().Name,
                        providerName);
                    configurator.Seed(modelBuilder);
                }
            }

            observability.RecordSuccess(providerName, "model_creating");
            logger?.LogDebug(
                "Database model configuration completed for provider {ProviderName}.",
                providerName);

        }
        catch (Exception ex)
        {
            observability.RecordFailure(providerName, "model_creating");
            logger?.LogError(
                ex,
                "Database model configuration failed for provider {ProviderName}.",
                providerName);
            throw new Exception("Failed to configure database model", ex);
            
        }
    }
    
    private List<IConfigurator> GetConfigurators()
    {
        return ConfiguratorProvider.Provide(serviceProvider, modelAssemblyRegistry.GetAll());
    }

    private IEnumerable<IDatabaseProviderModelConfigurator> GetProviderConfigurators(string providerName) =>
        serviceProvider.GetServices<IDatabaseProviderModelConfigurator>()
            .Where(configurator => string.Equals(configurator.ProviderName, providerName, StringComparison.Ordinal));
    
    
    /// <summary>
    /// Builds the cache key fragment representing the active provider, seed mode, and applied configurators.
    /// </summary>
    /// <returns>The model cache key fragment for the current context instance.</returns>
    public string GetModelCacheKey(string providerName)
    {
        var configuratorKeys = GetConfigurators()
            .Select(static configurator => configurator.ModelCacheKey)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        logger?.LogDebug(
            "Created database model cache key for provider {ProviderName}. Configurator key count: {ConfiguratorKeyCount}. Seed enabled: {SeedEnabled}.",
            providerName,
            configuratorKeys.Length,
            config.EnableSeed);

        return string.Join(
            "|",
            new[] { providerName, config.EnableSeed.ToString() }
                .Concat(GetProviderConfigurators(providerName)
                    .Select(static configurator => System.Text.Json.JsonSerializer.Serialize(new[]
                    {
                        configurator.GetType().FullName, configurator.ModelCacheKey
                    })))
                .Concat(configuratorKeys));
    }
}
