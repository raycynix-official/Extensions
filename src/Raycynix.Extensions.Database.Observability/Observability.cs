using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Observability;

/// <summary>
/// Provides Raycynix database observability registration extensions.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Enables database tracing and metrics integration for the Raycynix database infrastructure.
    /// </summary>
    /// <param name="builder">The database builder to extend.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddObservability(this IDatabaseBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.Replace(ServiceDescriptor.Singleton<IDatabaseObservability, DatabaseObservability>());
        return builder;
    }
}