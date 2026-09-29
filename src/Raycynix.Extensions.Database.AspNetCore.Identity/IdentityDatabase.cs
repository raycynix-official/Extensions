using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;

namespace Raycynix.Extensions.Database.AspNetCore.Identity;

/// <summary>
/// Provides service registration extensions for the Raycynix identity database infrastructure.
/// </summary>
public static class IdentityDatabase
{
    /// <summary>
    /// Extends service collections with Raycynix database registration APIs.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the Raycynix database infrastructure using separate marker types for model configurators and EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix Identity database context type to register.</typeparam>
        /// <typeparam name="TMarker">A marker type from the assembly that contributes EF Core configurators.</typeparam>
        /// <typeparam name="TMigrationMarker">A marker type from the assembly that contains EF Core migrations.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="contextName">An optional name under <c>DatabaseOptions:Contexts</c>.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext, TMarker, TMigrationMarker>(
            IConfiguration configuration,
            Action<DatabaseOptions>? setup = null,
            string? contextName = null)
            where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            var migrationsAssembly = typeof(TMigrationMarker).Assembly;
            var modelAssembly = typeof(TMarker).Assembly;

            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                migrationsAssembly,
                setup,
                modelAssembly,
                contextName: contextName);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type and a marker type
        /// from the assembly that contributes EF Core configurators and contains EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix Identity database context type to register.</typeparam>
        /// <typeparam name="TMarker">A marker type from the assembly to register for configurators and migrations.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="contextName">An optional name under <c>DatabaseOptions:Contexts</c>.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext, TMarker>(
            IConfiguration configuration,
            Action<DatabaseOptions>? setup = null,
            string? contextName = null)
            where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            var assembly = typeof(TMarker).Assembly;

            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                assembly,
                setup,
                assembly,
                contextName: contextName);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using explicit model and migrations assemblies.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix Identity database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="migrationsAssembly">The assembly that contains EF Core migrations.</param>
        /// <param name="modelAssembly">The assembly that contributes EF Core configurators.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="contextName">An optional name under <c>DatabaseOptions:Contexts</c>.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext>(
            IConfiguration configuration,
            Assembly migrationsAssembly,
            Assembly modelAssembly,
            Action<DatabaseOptions>? setup = null,
            string? contextName = null)
            where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(migrationsAssembly);
            ArgumentNullException.ThrowIfNull(modelAssembly);

            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                migrationsAssembly,
                setup,
                modelAssembly,
                contextName: contextName);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type and an explicit assembly
        /// that contributes EF Core configurators and contains EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix Identity database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="assembly">The assembly to register for configurators and migrations.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="contextName">An optional name under <c>DatabaseOptions:Contexts</c>.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext>(
            IConfiguration configuration,
            Assembly assembly,
            Action<DatabaseOptions>? setup = null,
            string? contextName = null)
            where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(assembly);

            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                assembly,
                setup,
                assembly,
                contextName: contextName);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix Identity database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="registerCallerAssembly">
        /// The entry or caller assembly is always used as the default EF Core migrations assembly.
        /// When this value is <see langword="true"/>, that assembly is also scanned for configurators.
        /// Disable this when assemblies should be registered explicitly.
        /// </param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext>(IConfiguration configuration,
            Action<DatabaseOptions>? setup = null,
            bool registerCallerAssembly = true) where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                callerAssembly,
                setup,
                registerCallerAssembly ? callerAssembly : null);
        }

        /// <summary>
        /// Registers an Identity context using its own configuration section.
        /// </summary>
        public IDatabaseBuilder AddRaycynixIdentityDatabase<TContext>(
            IConfiguration configuration,
            string contextName,
            Action<DatabaseOptions>? setup = null,
            bool registerCallerAssembly = true)
            where TContext : DbContext, IRaycynixIdentityDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

            var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            return DatabaseRegistrationExtensions.RegisterRaycynixDatabaseCore<TContext>(
                services,
                configuration,
                callerAssembly,
                setup,
                registerCallerAssembly ? callerAssembly : null,
                nameof(DatabaseOptions),
                contextName);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using the default <see cref="RaycynixIdentityDatabaseContext"/>.
        /// </summary>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseOptions"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="registerCallerAssembly">
        /// The entry or caller assembly is always used as the default EF Core migrations assembly.
        /// When this value is <see langword="true"/>, that assembly is also scanned for configurators.
        /// Disable this when assemblies should be registered explicitly.
        /// </param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase(IConfiguration configuration,
            Action<DatabaseOptions>? setup = null, bool registerCallerAssembly = true)
        {
            return services.AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext>(
                configuration,
                setup,
                registerCallerAssembly);
        }

        /// <summary>
        /// Registers the default Identity context with overrides from
        /// <c>DatabaseOptions:Contexts:&lt;contextName&gt;</c>.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="contextName">The logical context name.</param>
        /// <param name="setup">An optional final configuration callback.</param>
        /// <param name="registerCallerAssembly">Whether to scan the application assembly for configurators.</param>
        /// <returns>A builder scoped to the default Identity context.</returns>
        public IDatabaseBuilder AddRaycynixIdentityDatabase(
            IConfiguration configuration,
            string contextName,
            Action<DatabaseOptions>? setup = null,
            bool registerCallerAssembly = true)
        {
            return services.AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext>(
                configuration,
                contextName,
                setup,
                registerCallerAssembly);
        }

        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixDatabaseAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assembly);

            var pendingRegistry = DatabaseModelAssemblyRegistry.GetOrCreate(services);
            pendingRegistry.Add(assembly);

            var contextRegistries = services
                .Where(static descriptor => descriptor.ServiceType == typeof(IDatabaseModelAssemblyRegistry))
                .Select(static descriptor => descriptor.IsKeyedService
                    ? descriptor.KeyedImplementationInstance
                    : descriptor.ImplementationInstance)
                .OfType<DatabaseModelAssemblyRegistry>()
                .Distinct()
                .ToArray();

            foreach (var registry in contextRegistries)
            {
                registry.Add(assembly);
            }

            return services;
        }

        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <typeparam name="TMarker">A marker type from the assembly to register.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixDatabaseAssembly<TMarker>()
        {
            return services.AddRaycynixDatabaseAssembly(typeof(TMarker).Assembly);
        }
    }
}