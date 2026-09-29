using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Stores assemblies that contribute EF Core configurators to the shared database context.
/// </summary>
public sealed class DatabaseModelAssemblyRegistry : IDatabaseModelAssemblyRegistry
{
    private readonly Lock _sync = new();
    private readonly HashSet<Assembly> _assemblies = [];

    /// <summary>
    /// Adds an assembly to the registry.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    public void Add(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        lock (_sync)
            _assemblies.Add(assembly);
    }

    /// <summary>
    /// Gets the registered model assemblies.
    /// </summary>
    /// <returns>The unique assemblies that should be scanned for configurators.</returns>
    public IReadOnlyCollection<Assembly> GetAll()
    {
        lock (_sync)
            return [.. _assemblies];
    }

    /// <summary>
    /// Gets the existing shared registry from the service collection or creates and registers a new one.
    /// </summary>
    /// <param name="services">The service collection that owns the registry.</param>
    /// <param name="serviceKey">The optional context key that owns an isolated registry.</param>
    /// <returns>The shared model assembly registry instance.</returns>
    public static DatabaseModelAssemblyRegistry GetOrCreate(IServiceCollection services, object? serviceKey = null)
    {
        var serviceType = serviceKey is null
            ? typeof(DatabaseModelAssemblyRegistry)
            : typeof(IDatabaseModelAssemblyRegistry);
        var descriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == serviceType
                                                               && Equals(descriptor.ServiceKey, serviceKey));
        var implementation = serviceKey is null
            ? descriptor?.ImplementationInstance
            : descriptor?.KeyedImplementationInstance;
        if (implementation is DatabaseModelAssemblyRegistry existingRegistry)
        {
            return existingRegistry;
        }

        var registry = new DatabaseModelAssemblyRegistry();
        if (serviceKey is null)
        {
            services.AddSingleton(registry);
        }
        else
        {
            services.AddKeyedSingleton<IDatabaseModelAssemblyRegistry>(serviceKey, registry);
        }

        return registry;
    }
}