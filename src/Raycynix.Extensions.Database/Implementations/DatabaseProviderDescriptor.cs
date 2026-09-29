using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Captures the single resolved database provider registration used by the shared database infrastructure.
/// </summary>
public sealed class DatabaseProviderDescriptor
{
    /// <summary>
    /// Gets the normalized logical name of the active provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Gets the provider-specific registration implementation.
    /// </summary>
    public required IDatabaseProviderRegistration Registration { get; init; }

    /// <summary>
    /// Resolves the single active database provider registration from the service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing database provider registrations.</param>
    /// <param name="serviceKey">The optional context key used to select provider registrations.</param>
    /// <returns>A descriptor for the active database provider.</returns>
    /// <exception cref="NotSupportedException">Thrown when no database provider is registered.</exception>
    /// <exception cref="InvalidOperationException">Thrown when more than one database provider is registered.</exception>
    public static DatabaseProviderDescriptor Resolve(IServiceProvider serviceProvider, object? serviceKey = null)
    {
        var logger = serviceProvider.GetService<ILogger<DatabaseProviderDescriptor>>();
        var registrations = serviceKey is null
            ? serviceProvider.GetServices<IDatabaseProviderRegistration>().ToArray()
            : serviceProvider.GetKeyedServices<IDatabaseProviderRegistration>(serviceKey).ToArray();

        logger?.LogDebug(
            "Resolving database provider. Registered provider count: {ProviderCount}",
            registrations.Length);

        return registrations.Length switch
        {
            1 => Create(registrations[0], logger),
            0 => throw new NotSupportedException(
                "No database provider is registered. Add exactly one matching provider package, for example AddSqlite(), AddPostgreSql(), AddMsSql(), or AddMySql()."),
            _ => throw new InvalidOperationException(
                $"Multiple database providers are registered ({string.Join(", ", registrations.Select(static registration => registration.ProviderName))}). Register exactly one database provider package.")
        };
    }

    private static DatabaseProviderDescriptor Create(
        IDatabaseProviderRegistration registration,
        ILogger<DatabaseProviderDescriptor>? logger)
    {
        logger?.LogDebug(
            "Resolved active database provider {ProviderName}",
            registration.ProviderName);

        return new DatabaseProviderDescriptor
        {
            ProviderName = registration.ProviderName,
            Registration = registration
        };
    }
}