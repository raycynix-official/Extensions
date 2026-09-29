namespace Raycynix.Extensions.Database.Abstractions.Options;

/// <summary>
/// Represents the database settings used by the Raycynix database extensions.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Gets the raw connection string.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets the structured connection settings used when a raw connection string is not supplied.
    /// </summary>
    public ConnectionOptions? ConnectionOptions { get; set; }

    /// <summary>
    /// Gets a value indicating whether EF Core migrations should be applied during initialization.
    /// </summary>
    public bool UseMigrations { get; set; } = false;

    /// <summary>
    /// Gets or sets the migrations history table used by this context.
    /// Set a distinct value for each context that manages the same physical database.
    /// </summary>
    public string? MigrationsHistoryTable { get; set; }

    /// <summary>
    /// Gets or sets the optional schema containing the migrations history table.
    /// </summary>
    public string? MigrationsHistorySchema { get; set; }

    /// <summary>
    /// Gets a value indicating whether the database should be created when it does not exist.
    /// </summary>
    public bool EnsureCreated { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether entity seed logic should run during model creation.
    /// </summary>
    public bool EnableSeed { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether EF Core lazy loading is enabled for the shared context.
    /// </summary>
    public bool EnableLazyLoading { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether EF Core automatic change detection is enabled.
    /// </summary>
    public bool EnableAutoDetectChanges { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether queries are tracked by default.
    /// </summary>
    public bool UseQueryTrackingByDefault { get; set; } = true;

    /// <summary>
    /// Gets the maximum number of retry attempts for transient database failures.
    /// </summary>
    public int RetryCount { get; set; } = 5;

    /// <summary>
    /// Gets the delay, in seconds, between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 10;

    /// <summary>
    /// Validates provider-agnostic database settings and throws when incompatible or incomplete values are provided.
    /// </summary>
    public void Validate()
    {
        if (RetryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryCount), "Retry count cannot be negative.");
        }

        if (RetryDelaySeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryDelaySeconds), "Retry delay cannot be negative.");
        }

        if (EnsureCreated && UseMigrations)
        {
            throw new InvalidOperationException(
                "EnsureCreated and UseMigrations cannot both be enabled at the same time.");
        }

        if (MigrationsHistoryTable is not null && string.IsNullOrWhiteSpace(MigrationsHistoryTable))
        {
            throw new ArgumentException(
                "Migrations history table cannot be empty or whitespace.",
                nameof(MigrationsHistoryTable));
        }

        if (MigrationsHistorySchema is not null && string.IsNullOrWhiteSpace(MigrationsHistorySchema))
        {
            throw new ArgumentException(
                "Migrations history schema cannot be empty or whitespace.",
                nameof(MigrationsHistorySchema));
        }

        if (MigrationsHistorySchema is not null && MigrationsHistoryTable is null)
        {
            throw new InvalidOperationException(
                "MigrationsHistoryTable must be configured when MigrationsHistorySchema is set.");
        }

        var hasConnectionString = !string.IsNullOrWhiteSpace(ConnectionString);
        var hasConnectionOptions = ConnectionOptions is not null;

        if (!hasConnectionString && !hasConnectionOptions)
        {
            throw new InvalidOperationException(
                "Either ConnectionString or ConnectionOptions must be provided.");
        }

        if (hasConnectionOptions)
        {
            ConnectionOptions!.Validate("Database");
        }
    }
}
