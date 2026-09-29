namespace Raycynix.Extensions.Database.MySql.Options;

/// <summary>
/// Represents MySQL-specific connection settings.
/// </summary>
public sealed class MySqlOptions
{
    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether user variables are allowed.
    /// </summary>
    public bool AllowUserVariables { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; set; } = true;

    /// <summary>
    /// Validates MySQL-specific settings.
    /// </summary>
    public void Validate()
    {
        if (CommandTimeoutSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CommandTimeoutSeconds), "Command timeout cannot be negative.");
        }
    }
}