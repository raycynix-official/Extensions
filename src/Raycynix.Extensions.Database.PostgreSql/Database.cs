using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.PostgreSql.Options;
using Raycynix.Extensions.Database.PostgreSql.Internal;

namespace Raycynix.Extensions.Database.PostgreSql;

/// <summary>
/// Provides PostgreSQL-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds PostgreSQL provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting PostgreSQL-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddPostgreSql(
        this IDatabaseBuilder builder,
        Action<PostgreSqlOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var optionsName = builder.OptionsName;
        builder.Services.AddRaycynixConfiguration<PostgreSqlOptions>(
            builder.Configuration,
            $"{builder.ConfigurationSectionName}:{nameof(PostgreSqlOptions)}",
            optionsName: optionsName,
            configurePostBind: builder.ContextName is null ? configure : null);
        if (builder.ContextConfigurationSectionName is { } contextSectionName)
        {
            builder.Services.AddOptions<PostgreSqlOptions>(optionsName)
                .Bind(builder.Configuration.GetSection($"{contextSectionName}:{nameof(PostgreSqlOptions)}"))
                .PostConfigure(options => configure?.Invoke(options));
        }

        builder.Services.AddRaycynixConfigurationValidator<PostgreSqlOptions, PostgreSqlOptionsValidator>();
        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<PostgreSqlOptions>>().Get(optionsName)
        );

        builder.Services.AddKeyedSingleton<IDatabaseProviderRegistration>(
            builder.ContextType, (serviceProvider, _) =>
                new PostgreSqlDatabaseProviderRegistration(
                    serviceProvider.GetRequiredService<IOptionsMonitor<PostgreSqlOptions>>().Get(optionsName),
                    serviceProvider.GetService<ILogger<PostgreSqlDatabaseProviderRegistration>>()
                )
        );

        builder.Services.AddKeyedSingleton<IDatabaseProviderModelConfigurator>(
            builder.ContextType, (serviceProvider, _) =>
                new PostgreSqlProviderModelConfigurator(
                    serviceProvider.GetRequiredService<IOptionsMonitor<PostgreSqlOptions>>().Get(optionsName)
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