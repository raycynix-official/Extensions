using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Exposes the shared state used by Raycynix database builder extensions.
/// </summary>
public interface IDatabaseBuilder
{
    /// <summary>
    /// Gets the context type owned by this registration.
    /// </summary>
    public Type ContextType { get; }

    /// <summary>
    /// Gets the named-options key used by this context.
    /// </summary>
    public string OptionsName { get; }

    /// <summary>
    /// Gets the configuration section bound to this context.
    /// </summary>
    public string ConfigurationSectionName { get; }

    /// <summary>
    /// Gets the logical context name used for context-specific configuration overrides.
    /// </summary>
    public string? ContextName { get; }

    /// <summary>
    /// Gets the context-specific configuration section, when a context name was supplied.
    /// </summary>
    public string? ContextConfigurationSectionName { get; }

    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the application configuration used for database registrations.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Gets the primary assembly selected during database registration.
    /// This assembly is used as the default EF Core migrations assembly.
    /// </summary>
    public Assembly CallerAssembly { get; }
}
