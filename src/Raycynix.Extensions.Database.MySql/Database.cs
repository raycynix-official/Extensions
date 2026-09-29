using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MySql.Options;
using Raycynix.Extensions.Database.MySql.Internal;

namespace Raycynix.Extensions.Database.MySql;

/// <summary>
/// Provides MySQL-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds MySQL provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting MySQL-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddMySql(
        this IDatabaseBuilder builder,
        Action<MySqlOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var optionsName = builder.OptionsName;
        builder.Services.AddRaycynixConfiguration<MySqlOptions>(
            builder.Configuration,
            $"{builder.ConfigurationSectionName}:{nameof(MySqlOptions)}",
            optionsName: optionsName,
            configurePostBind: builder.ContextName is null ? configure : null);
        if (builder.ContextConfigurationSectionName is { } contextSectionName)
        {
            builder.Services.AddOptions<MySqlOptions>(optionsName)
                .Bind(builder.Configuration.GetSection($"{contextSectionName}:{nameof(MySqlOptions)}"))
                .PostConfigure(options => configure?.Invoke(options));
        }

        builder.Services.AddRaycynixConfigurationValidator<MySqlOptions, MySqlOptionsValidator>();
        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<MySqlOptions>>().Get(optionsName));

        builder.Services.AddKeyedSingleton<IDatabaseProviderRegistration>(
            builder.ContextType, (serviceProvider, _) =>
                new MySqlDatabaseProviderRegistration(
                    serviceProvider.GetRequiredService<IOptionsMonitor<MySqlOptions>>().Get(optionsName),
                    serviceProvider.GetService<ILogger<MySqlDatabaseProviderRegistration>>())
        );
        if (optionsName == Microsoft.Extensions.Options.Options.DefaultName)
        {
            builder.Services.AddSingleton<IDatabaseProviderRegistration>(serviceProvider =>
                serviceProvider.GetRequiredKeyedService<IDatabaseProviderRegistration>(builder.ContextType)
            );
        }

        return builder;
    }
}