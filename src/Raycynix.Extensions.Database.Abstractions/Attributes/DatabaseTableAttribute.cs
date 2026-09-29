namespace Raycynix.Extensions.Database.Abstractions.Attributes;

/// <summary>
/// Declares the default table name for an EF Core configurator.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DatabaseTableAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the table name declared for the configurator.
    /// </summary>
    public string Name { get; } = string.IsNullOrWhiteSpace(name)
        ? throw new ArgumentException("Table name cannot be null or whitespace.", nameof(name))
        : name;
}