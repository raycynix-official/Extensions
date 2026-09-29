using System.Reflection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.Infrastructure;

/// <summary>
/// Provides fluent extensions for adding model assemblies to a Raycynix database registration.
/// </summary>
public static class DatabaseBuilderExtensions
{
    /// <param name="builder">The database builder to extend.</param>
    extension(IDatabaseBuilder builder)
    {
        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        /// <returns>The same builder instance.</returns>
        public IDatabaseBuilder AddAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            DatabaseModelAssemblyRegistry.GetOrCreate(builder.Services, builder.ContextType).Add(assembly);
            return builder;
        }

        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <typeparam name="TMarker">A marker type from the assembly to register.</typeparam>
        /// <returns>The same builder instance.</returns>
        public IDatabaseBuilder AddAssembly<TMarker>()
        {
            return builder.AddAssembly(typeof(TMarker).Assembly);
        }
    }
}