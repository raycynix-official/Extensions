using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Provides an abstract base implementation for configuring database entities of a specified type.
/// </summary>
/// <typeparam name="T">
/// The type of the entity to be configured. Must be a reference type.
/// </typeparam>
public abstract class GenericConfigurator<T> : IGenericConfigurator<T> where T : class
{
    /// <summary>
    /// Gets the entity type handled by the current configurator.
    /// </summary>
    public Type Type => typeof(T);

    /// <summary>
    /// Gets the entity types whose configurators must be applied before the current one.
    /// </summary>
    public abstract Type[] DependsOn { get; }

    /// <summary>
    /// Gets the cache key fragment that identifies the model shape produced by the configurator.
    /// </summary>
    public virtual string ModelCacheKey
    {
        get
        {
            var shapeKey = GetModelShapeCacheKey();
            return string.IsNullOrWhiteSpace(shapeKey)
                ? Type.FullName ?? Type.Name
                : $"{Type.FullName ?? Type.Name}:{shapeKey}";
        }
    }

    /// <summary>
    /// Applies the default model configuration for <typeparamref name="T"/>.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> used to configure the entity mapping.
    /// </param>
    public virtual void Configure(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder);
    }

    /// <summary>
    /// Seeds data for <typeparamref name="T"/> during model creation.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> used to register seed data.
    /// </param>
    public virtual void Seed(ModelBuilder modelBuilder)
    {
    }

    /// <summary>
    /// Gets the configured entity builder and applies the resolved table name.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entity mapping.</param>
    /// <param name="tableName">An optional runtime table name override.</param>
    /// <returns>The configured entity builder.</returns>
    protected EntityTypeBuilder<T> ConfigureEntity(ModelBuilder modelBuilder, string? tableName = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var entityBuilder = modelBuilder.Entity<T>();
        entityBuilder.ToTable(ResolveTableName(tableName));
        var schema = GetType().GetCustomAttribute<DatabaseSchemaAttribute>()?.Name;
        if (schema is not null)
        {
            entityBuilder.Metadata.SetSchema(schema);
        }

        return entityBuilder;
    }

    /// <summary>
    /// Resolves the table name from an explicit override, a configurator attribute, or the entity type name.
    /// </summary>
    /// <param name="tableName">An optional runtime table name override.</param>
    /// <returns>The resolved table name.</returns>
    protected string ResolveTableName(string? tableName = null)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            return tableName;
        }

        return GetType().GetCustomAttribute<DatabaseTableAttribute>()?.Name ?? typeof(T).Name;
    }

    /// <summary>
    /// Returns the runtime model-shape discriminator that should participate in EF Core model caching.
    /// </summary>
    /// <returns>The additional cache key fragment for runtime-dependent model shape, or an empty value when not needed.</returns>
    protected virtual string? GetModelShapeCacheKey()
    {
        return null;
    }
}