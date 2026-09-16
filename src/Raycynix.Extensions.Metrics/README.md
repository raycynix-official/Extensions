# Raycynix.Extensions.Metrics

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package provides provider-neutral metrics registration for .NET 10 applications.

## What it contains

- `AddRaycynixMetrics()` for registering the standard `IMeterFactory`
- integration with the Microsoft.Extensions metrics pipeline
- the shared Raycynix meter from `Raycynix.Extensions.Metrics.Abstractions`
- compatibility with OpenTelemetry, OTLP, Aspire, Prometheus, and custom `MeterListener` consumers

The package no longer owns a Prometheus registry, metrics endpoint, health checks, or application configuration.
Export and transport are host concerns.

## Registration

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixMetrics();
```

The overload accepts the standard Microsoft metrics builder:

```csharp
builder.Services.AddRaycynixMetrics(metrics =>
{
    metrics.EnableMetrics("Raycynix.Extensions");
});
```

## Creating instruments

```csharp
using System.Diagnostics.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;

public sealed class CheckoutMetrics
{
    private readonly Counter<long> _orders;
    private readonly Histogram<double> _duration;

    public CheckoutMetrics(IMeterFactory meterFactory)
    {
        var meter = RaycynixMetrics.CreateMeter(meterFactory);

        _orders = meter.CreateCounter<long>(
            "raycynix.checkout.orders",
            unit: "{order}",
            description: "Number of processed checkout orders.");

        _duration = meter.CreateHistogram<double>(
            "raycynix.checkout.duration",
            unit: "s",
            description: "Checkout processing duration.");
    }

    public IDisposable Measure(string result) => _duration.MeasureDuration(
        new KeyValuePair<string, object?>("raycynix.checkout.result", result));

    public void RecordSuccess() => _orders.Add(
        1,
        new KeyValuePair<string, object?>("raycynix.checkout.result", "success"));
}
```

Prefer stable, bounded tag values. Do not use request IDs, user IDs, arbitrary URLs, or other unbounded values as metric tags.

## Aspire and OTLP

Raycynix instruments use `System.Diagnostics.Metrics`, so an Aspire resource can export them through its normal OpenTelemetry/OTLP setup. Register the Raycynix meter in the resource process; the AppHost does not need to proxy or recreate resource metrics.

For ASP.NET Core instrumentation and exporters, use `Raycynix.Extensions.Metrics.AspNetCore`.

## Migration from 2.x

Version 3.0 removes `IMetricsService`, `IMetricCounter`, `IMetricGauge`, `IMetricHistogram`, and `MetricsConfiguration`.

- inject `IMeterFactory` instead of `IMetricsService`
- create standard `Counter<T>`, `UpDownCounter<T>`, `ObservableGauge<T>`, and `Histogram<T>` instruments
- use named `KeyValuePair<string, object?>` tags instead of positional label arrays
- configure OTLP or Prometheus in the host
- register health checks independently
