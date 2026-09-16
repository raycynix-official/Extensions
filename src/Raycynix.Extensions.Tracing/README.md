# Raycynix.Extensions.Tracing

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package provides provider-neutral tracing registration for .NET 10 applications.

## What it contains

- `AddRaycynixTracing()` registration
- the standard shared `ActivitySource` from `Raycynix.Extensions.Tracing.Abstractions`
- compatibility with `ActivityListener`, OpenTelemetry, OTLP, and Aspire

The package does not own a tracer provider or exporter. Transport and sampling are configured by the host.

## Registration

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixTracing();
```

## Creating activities

```csharp
using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;

using var activity = RaycynixTracing.ActivitySource.StartActivity(
    "orders.process",
    ActivityKind.Internal);

activity?.SetTag("raycynix.order.id", orderId);
activity?.SetBaggage("raycynix.tenant", tenant);

try
{
    await ProcessAsync();
    activity?.SetStatus(ActivityStatusCode.Ok);
}
catch (Exception exception)
{
    activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    throw;
}
```

`StartActivity` returns `null` when no listener samples the source. Use null-conditional operations and avoid doing expensive tag preparation until an activity exists.

## Aspire and OpenTelemetry

Raycynix uses the stable source name `Raycynix.Extensions`. An Aspire resource can export these activities through its normal OpenTelemetry/OTLP pipeline. Configure collection in the resource process; the AppHost does not recreate spans emitted by its resources.

For ASP.NET Core request instrumentation and exporter composition, use `Raycynix.Extensions.Tracing.AspNetCore`.

## Migration from 2.x

Version 3.0 removes `ITracer` and `Tracer`.

- use `RaycynixTracing.ActivitySource.StartActivity(...)` instead of `ITracer.StartTrace(...)`
- use `Activity.SetTag(...)` instead of `ITracer.AddTag(...)`
- use `Activity.SetBaggage(...)` and `Activity.GetBaggageItem(...)`
- configure sampling and exporters through OpenTelemetry or `ActivityListener`
