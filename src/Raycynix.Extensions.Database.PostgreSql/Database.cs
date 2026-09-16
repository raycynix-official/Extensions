using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        builder.Services.AddRaycynixConfiguration<PostgreSqlOptions>(
            builder.Configuration,
            $"{nameof(DatabaseOptions)}:{nameof(PostgreSqlOptions)}",
            configurePostBind: configure);
        builder.Services.AddRaycynixConfigurationValidator<PostgreSqlOptions, PostgreSqlOptionsValidator>();

        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<PostgreSqlOptions>>().Current);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDatabaseProviderRegistration, PostgreSqlDatabaseProviderRegistration>());
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDatabaseProviderModelConfigurator, PostgreSqlProviderModelConfigurator>());

        return builder;
    }
}
