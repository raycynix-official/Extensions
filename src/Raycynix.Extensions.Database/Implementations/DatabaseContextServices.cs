using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Internal;

namespace Raycynix.Extensions.Database.Implementations;

internal sealed class DatabaseContextServices<TContext> : IDatabaseContextServices<TContext>
    where TContext : DbContext, IRaycynixDatabaseContext
{
    private readonly IDatabaseModelConfigurator _modelConfigurator;

    public DatabaseContextServices(
        IServiceProvider serviceProvider,
        IOptionsMonitor<DatabaseOptions> options,
        DatabaseContextDescriptor<TContext> descriptor)
    {
        Options = options.Get(descriptor.OptionsName);
        var providerDescriptor = DatabaseProviderDescriptor.Resolve(serviceProvider, descriptor.ContextType);
        ProviderName = providerDescriptor.ProviderName;

        _modelConfigurator = serviceProvider
            .GetRequiredKeyedService<IDatabaseModelConfigurator>(descriptor.ContextType);
    }

    public DatabaseOptions Options { get; }

    public string ProviderName { get; }

    public void ConfigureModel(ModelBuilder modelBuilder) =>
        _modelConfigurator.Configure(modelBuilder, ProviderName);

    public string GetModelCacheKey() => _modelConfigurator.GetModelCacheKey(ProviderName);
}