using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MsSql.Options;
using Raycynix.Extensions.Database.MsSql.Internal;

namespace Raycynix.Extensions.Database.MsSql;

/// <summary>
/// Provides SQL Server-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds SQL Server provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting SQL Server-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddMsSql(
        this IDatabaseBuilder builder,
        Action<MsSqlServerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var optionsName = builder.OptionsName;
        builder.Services.AddRaycynixConfiguration<MsSqlServerOptions>(
            builder.Configuration,
            $"{builder.ConfigurationSectionName}:{nameof(MsSqlServerOptions)}",
            optionsName: optionsName,
            configurePostBind: builder.ContextName is null ? configure : null);
        if (builder.ContextConfigurationSectionName is { } contextSectionName)
        {
            builder.Services.AddOptions<MsSqlServerOptions>(optionsName)
                .Bind(builder.Configuration.GetSection($"{contextSectionName}:{nameof(MsSqlServerOptions)}"))
                .PostConfigure(options => configure?.Invoke(options));
        }

        builder.Services.AddRaycynixConfigurationValidator<MsSqlServerOptions, MsSqlServerOptionsValidator>();
        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<MsSqlServerOptions>>().Get(optionsName)
        );

        builder.Services.AddKeyedSingleton<IDatabaseProviderRegistration>(
            builder.ContextType, (serviceProvider, _) =>
                new MsSqlServerDatabaseProviderRegistration(
                    serviceProvider.GetRequiredService<IOptionsMonitor<MsSqlServerOptions>>().Get(optionsName),
                    serviceProvider.GetService<ILogger<MsSqlServerDatabaseProviderRegistration>>()
                )
        );

        builder.Services.AddKeyedSingleton<IDatabaseProviderModelConfigurator>(
            builder.ContextType, (serviceProvider, _) =>
                new MsSqlServerProviderModelConfigurator(
                    serviceProvider.GetRequiredService<IOptionsMonitor<MsSqlServerOptions>>().Get(optionsName)
                )
        );

        if (optionsName == Microsoft.Extensions.Options.Options.DefaultName)
        {
            builder.Services.AddSingleton<IDatabaseProviderRegistration>(serviceProvider =>
                serviceProvider.GetRequiredKeyedService<IDatabaseProviderRegistration>(builder.ContextType));
            builder.Services.AddSingleton<IDatabaseProviderModelConfigurator>(serviceProvider =>
                serviceProvider.GetRequiredKeyedService<IDatabaseProviderModelConfigurator>(builder.ContextType));
        }

        return builder;
    }
}