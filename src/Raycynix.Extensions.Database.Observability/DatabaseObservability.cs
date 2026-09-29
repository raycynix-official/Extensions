using System.Diagnostics.Metrics;
using System.Diagnostics;
using Raycynix.Extensions.Common.Disposables;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions;
using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Database.Observability;

/// <summary>
/// Coordinates optional tracing and metrics emission for database operations.
/// </summary>
internal sealed class DatabaseObservability : IDatabaseObservability
{
    private readonly Counter<long>? _operationCounter;
    private readonly Histogram<double>? _operationDuration;
    private readonly ILogger<DatabaseObservability>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseObservability"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve optional observability services.</param>
    public DatabaseObservability(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetService(typeof(ILogger<DatabaseObservability>)) as ILogger<DatabaseObservability>;
        if (serviceProvider.GetService(typeof(IMeterFactory)) is not IMeterFactory meterFactory)
        {
            _logger?.LogDebug(
                "Database observability initialized without metrics service");
            return;
        }

        var meter = RaycynixMetrics.CreateMeter(meterFactory);
        _operationCounter = meter.CreateCounter<long>(
            "raycynix.database.operations",
            unit: "{operation}",
            description: "Number of observed database operations.");

        _operationDuration = meter.CreateHistogram<double>(
            "raycynix.database.operation.duration",
            unit: "s",
            description: "Duration of observed database operations.");

        _logger?.LogDebug(
            "Database observability initialized. Metrics enabled: {MetricsEnabled}",
            true);
    }

    /// <summary>
    /// Starts timing and tracing for a database operation.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    /// <returns>A disposable scope that completes the timing and tracing operation.</returns>
    public IDisposable BeginOperation(string providerName, string operation)
    {
        providerName = providerName.ToLowerInvariant();
        _logger?.LogDebug(
            "Beginning observed database operation {Operation} for provider {ProviderName}",
            operation,
            providerName);

        var timer = _operationDuration?.MeasureDuration(
            new("raycynix.database.provider", providerName),
            new("raycynix.database.operation", operation)
        ) ?? NoopDisposable.Instance;

        var activity = RaycynixTracing.ActivitySource.StartActivity($"database.{operation}", ActivityKind.Client);
        activity?.SetTag("raycynix.database.provider", providerName);
        activity?.SetTag("raycynix.database.operation", operation);

        var trace = (IDisposable?)activity ?? NoopDisposable.Instance;

        return new CompositeDisposable(timer, trace);
    }

    /// <summary>
    /// Records a successful database operation result.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    public void RecordSuccess(string providerName, string operation)
    {
        Record(providerName, operation, "success");
    }

    /// <summary>
    /// Records a failed database operation result.
    /// </summary>
    /// <param name="providerName">The logical provider name.</param>
    /// <param name="operation">The logical operation name.</param>
    public void RecordFailure(string providerName, string operation)
    {
        Record(providerName, operation, "failure");
    }

    /// <summary>
    /// Adds a trace tag when tracing is enabled.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    public void AddTag(string key, string value)
    {
        Activity.Current?.SetTag(key, value);
    }

    private void Record(string providerName, string operation, string status)
    {
        Activity.Current?
            .SetTag("raycynix.database.status", status)
            .SetStatus(status == "failure" ? ActivityStatusCode.Error : ActivityStatusCode.Ok);

        _operationCounter?.Add(
            1,
            new("raycynix.database.provider", providerName.ToLowerInvariant()),
            new("raycynix.database.operation", operation),
            new("raycynix.database.status", status));

        _logger?.LogDebug(
            "Recorded database operation {Operation} for provider {ProviderName} with status {Status}",
            operation,
            providerName,
            status);
    }

    private sealed class CompositeDisposable(IDisposable first, IDisposable second) : IDisposable
    {
        public void Dispose()
        {
            second.Dispose();
            first.Dispose();
        }
    }
}