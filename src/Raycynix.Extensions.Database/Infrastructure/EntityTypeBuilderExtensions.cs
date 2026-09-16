using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raycynix.Extensions.Database.Infrastructure;

/// <summary>
/// Provides convenience extensions for EF Core entity builders used by the Raycynix database package.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <param name="entityBuilder">The entity builder to configure.</param>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    extension<T>(EntityTypeBuilder<T> entityBuilder) where T : class
    {
        /// <summary>
        /// Applies the specified table name to the current entity builder.
        /// </summary>
        /// <param name="tableName">The table name to apply.</param>
        /// <returns>The same entity builder instance.</returns>
        public EntityTypeBuilder<T> EntityName(string tableName)
        {
            ArgumentNullException.ThrowIfNull(entityBuilder);
            ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

            entityBuilder.Metadata.SetTableName(tableName);
            return entityBuilder;
        }

        /// <summary>
        /// Applies a table schema without changing the table name.
        /// Pass <see langword="null"/> to use the model or provider default schema.
        /// </summary>
        /// <param name="schema">The schema name, or null to use the default.</param>
        /// <returns>The same entity builder instance.</returns>
        public EntityTypeBuilder<T> EntitySchema(string? schema)
        {
            ArgumentNullException.ThrowIfNull(entityBuilder);
            if (schema is not null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(schema);
            }

            entityBuilder.Metadata.SetSchema(schema);
            return entityBuilder;
        }
    }
}
