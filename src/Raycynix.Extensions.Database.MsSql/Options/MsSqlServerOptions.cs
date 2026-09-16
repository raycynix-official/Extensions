namespace Raycynix.Extensions.Database.MsSql.Options;

/// <summary>
/// Represents SQL Server-specific connection settings.
/// </summary>
public sealed class MsSqlServerOptions
{
    /// <summary>
    /// Gets or sets the default entity schema. Null preserves the provider default.
    /// Explicit entity mappings override this value. Migration history is unaffected.
    /// </summary>
    public string? DefaultSchema { get; set; }

    /// <summary>
    /// Gets a value indicating whether the server certificate should be trusted.
    /// </summary>
    public bool TrustServerCertificate { get; set; } = false;

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether multiple active result sets are enabled.
    /// </summary>
    public bool MultipleActiveResultSets { get; set; } = false;

    /// <summary>
    /// Validates SQL Server-specific settings.
    /// </summary>
    public void Validate()
    {
        if (DefaultSchema is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(DefaultSchema);
        }

        if (CommandTimeoutSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CommandTimeoutSeconds), "Command timeout cannot be negative.");
        }
    }
}
