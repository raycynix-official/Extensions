namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Captures the Raycynix database context type selected for the application.
/// </summary>
internal sealed class DatabaseContextDescriptor<TContext>
{
    /// <summary>
    /// Gets the concrete context type registered for the Raycynix database infrastructure.
    /// </summary>
    public required Type ContextType { get; init; }

    public required string OptionsName { get; init; }

    public required string ConfigurationSectionName { get; init; }

    public string? ContextName { get; init; }

    public required Implementations.DatabaseModelAssemblyRegistry ModelAssemblyRegistry { get; init; }
}