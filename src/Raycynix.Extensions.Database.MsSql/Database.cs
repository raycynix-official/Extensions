using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        builder.Services.AddRaycynixConfiguration<MsSqlServerOptions>(
            builder.Configuration,
            $"{nameof(DatabaseOptions)}:{nameof(MsSqlServerOptions)}",
            configurePostBind: configure);
        builder.Services.AddRaycynixConfigurationValidator<MsSqlServerOptions, MsSqlServerOptionsValidator>();

        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<MsSqlServerOptions>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderRegistration, MsSqlServerDatabaseProviderRegistration>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderModelConfigurator, MsSqlServerProviderModelConfigurator>());

        return builder;
    }
}
