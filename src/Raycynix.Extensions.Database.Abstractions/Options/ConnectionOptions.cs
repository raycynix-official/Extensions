namespace Raycynix.Extensions.Database.Abstractions.Options;

/// <summary>
/// Represents the base connection settings used to build a database connection string.
/// </summary>
public class ConnectionOptions
{
    /// <summary>
    /// Gets the host name or IP address of the database server.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets the database server port.
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Gets the database name or data source name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets the username used to authenticate the connection.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets the password used to authenticate the connection.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Validates provider-agnostic minimum connection values.
    /// Provider-specific requirements are validated by the selected database provider.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    public virtual void Validate(string providerName)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException($"{providerName} connection requires a database name.");
        }
    }
}