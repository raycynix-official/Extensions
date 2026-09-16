# Raycynix.Extensions.Metrics.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package contains shared conventions and helpers for provider-neutral Raycynix metrics.

## API

- `RaycynixMetrics.MeterName` — the shared meter name `Raycynix.Extensions`
- `RaycynixMetrics.MeterVersion` — the instrumentation release version
- `RaycynixMetrics.CreateMeter(IMeterFactory)` — creates a factory-managed meter
- `HistogramExtensions.MeasureDuration(...)` — records an elapsed duration in seconds when disposed

```csharp
using System.Diagnostics.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;

public sealed class OrderMetrics
{
    private readonly Counter<long> _processed;

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = RaycynixMetrics.CreateMeter(meterFactory);
        _processed = meter.CreateCounter<long>(
            "raycynix.orders.processed",
            unit: "{order}");
    }

    public void Record(string status) => _processed.Add(
        1,
        new KeyValuePair<string, object?>("raycynix.order.status", status));
}
```

The 2.x custom metrics interfaces were removed. Consumers now depend directly on `System.Diagnostics.Metrics`, allowing any compatible listener or OpenTelemetry exporter to collect the same instruments.
