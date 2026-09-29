using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Provides the default no-op database observability implementation.
/// </summary>
internal sealed class NoOpDatabaseObservability : IDatabaseObservability
{
    public IDisposable BeginOperation(string providerName, string operation)
    {
        return NullScope.Instance;
    }

    public void AddTag(string key, string value)
    {
    }

    public void RecordSuccess(string providerName, string operation)
    {
    }

    public void RecordFailure(string providerName, string operation)
    {
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}