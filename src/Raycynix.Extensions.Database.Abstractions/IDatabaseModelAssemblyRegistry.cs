using System.Reflection;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Exposes the assemblies that contribute EF Core model configurators.
/// </summary>
public interface IDatabaseModelAssemblyRegistry
{
    /// <summary>
    /// Gets the registered model assemblies.
    /// </summary>
    /// <returns>The assemblies that should be scanned for configurators.</returns>
    IReadOnlyCollection<Assembly> GetAll();
}