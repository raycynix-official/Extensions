namespace Raycynix.Extensions.Database.Abstractions.Attributes;

/// <summary>
/// Declares the table schema for an EF Core configurator.
/// Without this attribute, the model or provider default schema is used.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DatabaseSchemaAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the schema name declared for the configurator.
    /// </summary>
    public string Name { get; } = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Schema name cannot be null or whitespace.", nameof(name))
        : name;
}