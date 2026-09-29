namespace Raycynix.Extensions.Database.Abstractions;

/// <summary>
/// Defines hooks for observing Raycynix database infrastructure operations.
/// </summary>
public interface IDatabaseObservability
{
    /// <summary>
    /// Starts observing a database operation.
    /// </summary>
    /// <param name="providerName">The logical database provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    /// <returns>A scope that completes the observation when disposed.</returns>
    IDisposable BeginOperation(string providerName, string operation);

    /// <summary>
    /// Adds an operation tag when the active observability implementation supports tags.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    void AddTag(string key, string value);

    /// <summary>
    /// Records a successful database operation.
    /// </summary>
    /// <param name="providerName">The logical database provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    void RecordSuccess(string providerName, string operation);

    /// <summary>
    /// Records a failed database operation.
    /// </summary>
    /// <param name="providerName">The logical database provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    void RecordFailure(string providerName, string operation);
}