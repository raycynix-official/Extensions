using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Internal;

namespace Raycynix.Extensions.Database.Infrastructure;

/// <summary>
/// Contains shared registration logic used by Raycynix database extension packages.
/// </summary>
public class DatabaseRegistrationExtensions
{
    /// <summary>
    /// Registers the shared Raycynix database infrastructure for the specified EF Core context type.
    /// </summary>
    /// <typeparam name="TContext">The concrete DbContext type to register.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind database settings.</param>
    /// <param name="migrationsAssembly">The assembly that contains EF Core migrations.</param>
    /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
    /// <param name="modelAssembly">An optional assembly that contributes EF Core model configurators.</param>
    /// <param name="sectionName">The shared database configuration section.</param>
    /// <param name="contextName">An optional name under the shared section's <c>Contexts</c> child.</param>
    /// <returns>A database builder for provider and feature registration.</returns>
    public static IDatabaseBuilder RegisterRaycynixDatabaseCore<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationsAssembly,
        Action<DatabaseOptions>? setup,
        Assembly? modelAssembly,
        string sectionName = nameof(DatabaseOptions),
        string? contextName = null)
        where TContext : DbContext, IRaycynixDatabaseContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(migrationsAssembly);

        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        if (contextName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        }

        var hasRegisteredContext = services.Any(static service =>
            service.ServiceType.IsGenericType &&
            service.ServiceType.GetGenericTypeDefinition() == typeof(DatabaseContextDescriptor<>));
        var newOptionsName = hasRegisteredContext
            ? typeof(TContext).AssemblyQualifiedName!
            : Options.DefaultName;
        var descriptorType = typeof(DatabaseContextDescriptor<TContext>);
        var existingDescriptor = services.FirstOrDefault(service => service.ServiceType == descriptorType)
            ?.ImplementationInstance as DatabaseContextDescriptor<TContext>;
        var optionsName = existingDescriptor?.OptionsName ?? newOptionsName;
        var modelAssemblyRegistry = existingDescriptor is null
            ? DatabaseModelAssemblyRegistry.GetOrCreate(services, typeof(TContext))
            : existingDescriptor.ModelAssemblyRegistry;
        if (modelAssembly is not null)
        {
            modelAssemblyRegistry.Add(modelAssembly);
        }

        if (existingDescriptor is not null)
        {
            if (setup is not null)
            {
                throw new InvalidOperationException(
                    "Raycynix database is already registered. Configure DatabaseOptions only on the first AddRaycynixDatabase call");
            }

            return new DatabaseBuilder(
                services,
                configuration,
                migrationsAssembly,
                typeof(TContext),
                optionsName,
                existingDescriptor.ConfigurationSectionName,
                existingDescriptor.ContextName);
        }

        services.AddRaycynixConfiguration<DatabaseOptions>(
            configuration,
            sectionName: sectionName,
            optionsName: optionsName,
            configurePostBind: contextName is null ? setup : null);
        if (contextName is not null)
        {
            services.AddOptions<DatabaseOptions>(optionsName)
                .Bind(configuration.GetSection($"{sectionName}:Contexts:{contextName}"))
                .PostConfigure(options => setup?.Invoke(options));
        }

        services.AddRaycynixConfigurationValidator<DatabaseOptions, DatabaseOptionsValidator>();
        services.TryAddSingleton<IDatabaseObservability, NoOpDatabaseObservability>();
        var contextDescriptor = new DatabaseContextDescriptor<TContext>
        {
            ContextType = typeof(TContext),
            OptionsName = optionsName,
            ConfigurationSectionName = sectionName,
            ContextName = contextName,
            ModelAssemblyRegistry = modelAssemblyRegistry
        };
        services.AddSingleton(contextDescriptor);
        services.AddScoped<IDatabaseContextServices<TContext>, DatabaseContextServices<TContext>>();
        services.AddKeyedScoped<IDatabaseModelConfigurator>(typeof(TContext), (serviceProvider, _) =>
        {
            var config = serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseOptions>>().Get(optionsName);
            return new DatabaseModelConfigurator(
                config,
                modelAssemblyRegistry,
                serviceProvider.GetRequiredService<IDatabaseObservability>(),
                serviceProvider,
                serviceProvider.GetKeyedServices<IDatabaseProviderModelConfigurator>(typeof(TContext)),
                serviceProvider.GetService<ILogger<DatabaseModelConfigurator>>());
        });

        services.AddSingleton<IDatabaseInitializer<TContext>>(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseOptions>>().Get(optionsName);
            var provider = DatabaseProviderDescriptor.Resolve(serviceProvider, typeof(TContext));
            return new DatabaseInitializer<TContext>(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                serviceProvider.GetRequiredService<IDatabaseObservability>(),
                config,
                provider,
                serviceProvider.GetService<ILogger<DatabaseInitializer<TContext>>>());
        });
        services.AddSingleton<IDatabaseInitializer>(serviceProvider =>
            serviceProvider.GetRequiredService<IDatabaseInitializer<TContext>>());

        if (services.All(static descriptor => descriptor.ServiceType != typeof(TContext)))
        {
            services.AddDbContext<TContext>((serviceProvider, options) =>
            {
                var logger = serviceProvider.GetService<ILogger<TContext>>();
                var config = serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseOptions>>().Get(optionsName);
                var providerDescriptor = DatabaseProviderDescriptor.Resolve(serviceProvider, typeof(TContext));
                var providerRegistration = providerDescriptor.Registration;

                logger?.LogDebug(
                    "Configuring DbContext {DbContextType} with database provider {ProviderName}. Migrations assembly: {MigrationsAssembly}",
                    typeof(TContext).Name,
                    providerDescriptor.ProviderName,
                    migrationsAssembly.GetName().Name);

                providerRegistration.Validate(config);
                logger?.LogDebug(
                    "Database configuration validated for DbContext {DbContextType} with provider {ProviderName}",
                    typeof(TContext).Name,
                    providerDescriptor.ProviderName);

                var connectionString = providerRegistration.ResolveConnectionString(config, serviceProvider);
                options.ReplaceService<IModelCacheKeyFactory, DatabaseModelCacheKeyFactory>();
                providerRegistration.Configure(options, connectionString, config, migrationsAssembly, serviceProvider);

                logger?.LogDebug(
                    "DbContext {DbContextType} configured with database provider {ProviderName}",
                    typeof(TContext).Name,
                    providerDescriptor.ProviderName);
            });
        }

        services.AddScoped<IRaycynixDatabaseContext>(provider =>
            provider.GetRequiredService<TContext>());

        // Keep the original unkeyed services available for existing single-context consumers.
        if (services.Count(service => service.ServiceType == typeof(IRaycynixDatabaseContext)) == 1)
        {
            services.TryAddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IOptionsMonitor<DatabaseOptions>>().Get(optionsName));
            services.TryAddSingleton<IDatabaseModelAssemblyRegistry>(modelAssemblyRegistry);
            services.TryAddScoped<IDatabaseModelConfigurator>(serviceProvider =>
                serviceProvider.GetRequiredKeyedService<IDatabaseModelConfigurator>(typeof(TContext)));
            services.TryAddSingleton(serviceProvider =>
                DatabaseProviderDescriptor.Resolve(serviceProvider, typeof(TContext)));
        }

        return new DatabaseBuilder(
            services, configuration, migrationsAssembly, typeof(TContext), optionsName, sectionName, contextName);
    }
}
