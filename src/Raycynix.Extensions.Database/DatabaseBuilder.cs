using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides a fluent API for extending the Raycynix database registration.
/// </summary>
public class DatabaseBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    Assembly registrationAssembly,
    Type contextType,
    string optionsName,
    string configurationSectionName,
    string? contextName) : IDatabaseBuilder
{
    /// <inheritdoc />
    public Type ContextType { get; } = contextType ?? throw new ArgumentNullException(nameof(contextType));

    /// <inheritdoc />
    public string OptionsName { get; } = optionsName ?? throw new ArgumentNullException(nameof(optionsName));

    /// <inheritdoc />
    public string ConfigurationSectionName { get; } = configurationSectionName ??
                                                      throw new ArgumentNullException(nameof(configurationSectionName));

    /// <inheritdoc />
    public string? ContextName { get; } = contextName;

    /// <inheritdoc />
    public string? ContextConfigurationSectionName { get; } = contextName is null
        ? null
        : $"{configurationSectionName}:Contexts:{contextName}";

    /// <inheritdoc />
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <inheritdoc />
    public IConfiguration Configuration { get; } =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <inheritdoc />
    public Assembly CallerAssembly { get; } =
        registrationAssembly ?? throw new ArgumentNullException(nameof(registrationAssembly));
}