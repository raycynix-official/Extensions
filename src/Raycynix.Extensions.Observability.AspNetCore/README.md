# Raycynix.Extensions.Observability.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Observability.AspNetCore` adds ASP.NET Core integration for Raycynix observability and includes the core observability registration.

## What it contains

- `AddRaycynixAspNetCoreObservability(...)`
- `UseRaycynixObservability(this IApplicationBuilder app)`
- `MapRaycynixObservabilityEndpoints(this IEndpointRouteBuilder endpoints)`
- OpenTelemetry ASP.NET Core request metrics registration
- OpenTelemetry ASP.NET Core request tracing registration
- correlation middleware and `HttpClient` correlation propagation

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreObservability();

var app = builder.Build();

app.UseRaycynixObservability();
app.MapRaycynixObservabilityEndpoints();

app.Run();
```

`AddRaycynixAspNetCoreObservability(...)` already calls `AddRaycynixObservability()`, so no extra core registration is required in ASP.NET Core applications.

The endpoint helper maps health checks only. You can select a custom health path:

```csharp
app.MapRaycynixObservabilityEndpoints("/internal/health");
```

Metrics export is explicit. For a Prometheus scraping endpoint:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddPrometheusExporter());

// after builder.Build()
app.UseOpenTelemetryPrometheusScrapingEndpoint(
    context => context.Request.Path == "/internal/metrics");
```

For Aspire and production OpenTelemetry pipelines, prefer OTLP export for both metrics and traces instead of adding a scraping endpoint to every resource.

ASP.NET Core tracing uses standard OpenTelemetry instrumentation and requires no custom tracing middleware. `UseRaycynixObservability()` only adds the Raycynix correlation context middleware. It does not require `Raycynix.Extensions.Serilog`.

By default, the request logging scope includes `CorrelationId`, `TraceId`, `UserId`, `SubjectId`, and `SubjectType` to preserve the previous enrichment behavior. Applications that do not want user or subject identifiers in log scopes can disable that part:

```csharp
builder.Services.AddRaycynixAspNetCoreObservability(options =>
{
    options.IncludeIdentityInLoggingScope = false;
});
```
