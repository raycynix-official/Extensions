# Raycynix.Extensions.Metrics.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package integrates Raycynix `System.Diagnostics.Metrics` instruments with OpenTelemetry and ASP.NET Core request instrumentation.

## OTLP and Aspire

Use the provider-neutral registration when the application exports through OTLP, including Aspire service-default pipelines:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreMetrics(metrics =>
{
    metrics.AddOtlpExporter(); // requires OpenTelemetry.Exporter.OpenTelemetryProtocol
});

var app = builder.Build();
app.Run();
```

`AddRaycynixAspNetCoreMetrics` registers the shared Raycynix meter, the standard `IMeterFactory`, and OpenTelemetry ASP.NET Core instrumentation. No metrics request middleware is required for OTLP export.

## Prometheus scraping

Install `OpenTelemetry.Exporter.Prometheus.AspNetCore` explicitly, then compose it through the standard callback:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreMetrics(metrics =>
    metrics.AddPrometheusExporter());

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
```

A custom exact path can be selected explicitly:

```csharp
app.UseOpenTelemetryPrometheusScrapingEndpoint(
    context => context.Request.Path == "/internal/metrics");
```

The OpenTelemetry Prometheus exporter is currently distributed as a prerelease component. Prefer OTLP and an OpenTelemetry Collector for production environments when that topology is available. The scraping endpoint is not secured automatically; protect it at the network or application layer.

## Migration from 2.x

- replace `AddRaycynixMetrics(configuration)` with `AddRaycynixAspNetCoreMetrics()` or `AddRaycynixPrometheusMetrics()`
- remove `UseRaycynixMetrics()` and `MapRaycynixMetrics()`
- for Prometheus, explicitly install its prerelease exporter and add the official scraping middleware
- move health-check registration and mapping to the application or observability composition
- remove the obsolete `MetricsConfiguration` section
