using Microsoft.Data.Sqlite;

namespace Raycynix.Extensions.Database.Sqlite.Options;

/// <summary>
/// Represents SQLite-specific connection settings.
/// </summary>
public sealed class SqliteOptions
{
    /// <summary>
    /// Gets the SQLite open mode.
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// Gets the SQLite cache mode.
    /// </summary>
    public string? Cache { get; set; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Validates SQLite-specific settings.
    /// </summary>
    public void Validate()
    {
        ValidateEnum<SqliteOpenMode>(Mode, nameof(Mode));
        ValidateEnum<SqliteCacheMode>(Cache, nameof(Cache));

        if (CommandTimeoutSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(CommandTimeoutSeconds), "Command timeout cannot be negative");
    }

    private static void ValidateEnum<TEnum>(string? value, string propertyName)
        where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            (!Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed)))
        {
            throw new InvalidOperationException($"{propertyName} contains unsupported value '{value}'");
        }
    }
}