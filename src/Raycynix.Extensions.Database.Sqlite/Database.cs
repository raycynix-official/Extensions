using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Sqlite.Options;
using Raycynix.Extensions.Database.Sqlite.Internal;

namespace Raycynix.Extensions.Database.Sqlite;

/// <summary>
/// Provides SQLite-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds SQLite provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting SQLite-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddSqlite(
        this IDatabaseBuilder builder,
        Action<SqliteOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var optionsName = builder.OptionsName;
        builder.Services.AddRaycynixConfiguration<SqliteOptions>(
            builder.Configuration,
            $"{builder.ConfigurationSectionName}:{nameof(SqliteOptions)}",
            optionsName: optionsName,
            configurePostBind: builder.ContextName is null ? configure : null);
        if (builder.ContextConfigurationSectionName is { } contextSectionName)
        {
            builder.Services.AddOptions<SqliteOptions>(optionsName)
                .Bind(builder.Configuration.GetSection($"{contextSectionName}:{nameof(SqliteOptions)}"))
                .PostConfigure(options => configure?.Invoke(options));
        }

        builder.Services.AddRaycynixConfigurationValidator<SqliteOptions, SqliteOptionsValidator>();
        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptionsMonitor<SqliteOptions>>().Get(optionsName)
        );

        builder.Services.AddKeyedSingleton<IDatabaseProviderRegistration>(
            builder.ContextType, (serviceProvider, _) =>
                new SqliteDatabaseProviderRegistration(
                    serviceProvider.GetRequiredService<IOptionsMonitor<SqliteOptions>>().Get(optionsName),
                    serviceProvider.GetService<ILogger<SqliteDatabaseProviderRegistration>>()
                )
        );
        if (optionsName == Microsoft.Extensions.Options.Options.DefaultName)
        {
            builder.Services.AddSingleton<IDatabaseProviderRegistration>(serviceProvider =>
                serviceProvider.GetRequiredKeyedService<IDatabaseProviderRegistration>(builder.ContextType));
        }

        return builder;
    }
}