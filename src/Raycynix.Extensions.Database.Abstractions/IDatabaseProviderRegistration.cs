using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines provider-specific database registration behavior.
/// </summary>
public interface IDatabaseProviderRegistration
{
    /// <summary>
    /// Gets the logical provider name handled by the registration.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Resolves the final provider-specific connection string.
    /// </summary>
    /// <param name="configuration">The bound shared database configuration.</param>
    /// <param name="serviceProvider">The service provider used to resolve provider-specific options.</param>
    /// <returns>The final connection string passed to EF Core.</returns>
    string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider);

    /// <summary>
    /// Applies provider-specific EF Core options to the shared database context.
    /// </summary>
    /// <param name="options">The EF Core options builder to configure.</param>
    /// <param name="connectionString">The resolved provider-specific connection string.</param>
    /// <param name="configuration">The bound shared database configuration.</param>
    /// <param name="migrationsAssembly">The assembly that contains EF Core migrations.</param>
    /// <param name="serviceProvider">The service provider used to resolve provider-specific options.</param>
    void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseOptions configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider);

    /// <summary>
    /// Validates provider-specific database configuration before connection-string resolution.
    /// </summary>
    /// <param name="configuration">The bound shared database configuration.</param>
    void Validate(DatabaseOptions configuration);
}