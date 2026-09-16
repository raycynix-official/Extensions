# Raycynix.Extensions.Tracing.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package integrates the shared Raycynix `ActivitySource` with OpenTelemetry ASP.NET Core request instrumentation.

## Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreTracing();

var app = builder.Build();
app.Run();
```

No custom tracing middleware is required. ASP.NET Core creates server activities, while standard `Microsoft.Extensions.Logging` activity tracking adds `TraceId` and `SpanId` to logging scopes.

## OTLP and Aspire

Add an exporter through the standard OpenTelemetry callback:

```csharp
builder.Services.AddRaycynixAspNetCoreTracing(tracing =>
{
    tracing.AddOtlpExporter(); // requires OpenTelemetry.Exporter.OpenTelemetryProtocol
});
```

Aspire resources can use the same registration with the OTLP exporter supplied by their service-default pipeline.

## Additional instrumentation

```csharp
builder.Services.AddRaycynixAspNetCoreTracing(tracing =>
{
    tracing.AddHttpClientInstrumentation();
});
```

Install the corresponding OpenTelemetry instrumentation package when adding optional components.

## Migration from 2.x

- replace `AddRaycynixTracing()` in web applications with `AddRaycynixAspNetCoreTracing()`
- remove `UseRaycynixTracing()`
- remove custom `TracingMiddleware` references
- configure exporters through the `TracerProviderBuilder` callback
