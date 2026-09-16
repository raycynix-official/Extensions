namespace Raycynix.Extensions.Database.PostgreSql.Options;

/// <summary>
/// Represents PostgreSQL-specific connection settings.
/// </summary>
public sealed class PostgreSqlOptions
{
    /// <summary>
    /// Gets or sets the default entity schema. Null preserves the provider default.
    /// Explicit entity mappings override this value. Migration history is unaffected.
    /// </summary>
    public string? DefaultSchema { get; set; }

    /// <summary>
    /// Gets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; set; } = true;

    /// <summary>
    /// Gets the minimum pool size.
    /// </summary>
    public int? MinimumPoolSize { get; set; }

    /// <summary>
    /// Gets the maximum pool size.
    /// </summary>
    public int? MaximumPoolSize { get; set; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether detailed provider errors are included.
    /// </summary>
    public bool IncludeErrorDetail { get; set; } = false;

    /// <summary>
    /// Validates PostgreSQL-specific settings.
    /// </summary>
    public void Validate()
    {
        if (DefaultSchema is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(DefaultSchema);
        }

        if (MinimumPoolSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumPoolSize), "Minimum pool size cannot be negative.");
        }

        if (MaximumPoolSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumPoolSize), "Maximum pool size must be greater than zero.");
        }

        if (MinimumPoolSize is not null &&
            MaximumPoolSize is not null &&
            MinimumPoolSize > MaximumPoolSize)
        {
            throw new InvalidOperationException("Minimum pool size cannot exceed maximum pool size.");
        }

        if (CommandTimeoutSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CommandTimeoutSeconds), "Command timeout cannot be negative.");
        }
    }
}
