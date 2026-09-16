# Raycynix.Extensions.Observability

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Observability` contains the core observability composition for Raycynix applications.

## What it contains

- `AddRaycynixObservability(...)`
- registration of tracing and the standard `IMeterFactory` metrics services
- `IOperationContext` registration

## What it does not contain

- ASP.NET Core middleware
- endpoint mapping
- `HttpClient` correlation propagation

## Usage

```csharp
builder.Services.AddRaycynixObservability();
```

Raycynix components emit provider-neutral `System.Diagnostics.Metrics` instruments. Configure an OpenTelemetry exporter in the host that collects the `Raycynix.Extensions` meter.

```csharp
public sealed class CheckoutHandler(
    IOperationContext operationContext,
    Microsoft.Extensions.Logging.ILogger<CheckoutHandler> logger)
{
    public void Handle()
    {
        using var activity = Raycynix.Extensions.Tracing.Abstractions.RaycynixTracing
            .ActivitySource.StartActivity("checkout.handle");

        logger.LogInformation(
            "Handling checkout. CorrelationId:{CorrelationId} TraceId:{TraceId}",
            operationContext.CorrelationId,
            operationContext.TraceId);
    }
}
```

`AddRaycynixObservability()` does not register `Raycynix.Extensions.Serilog`. Applications can use any provider that works with `Microsoft.Extensions.Logging`.

For ASP.NET Core integration, use `Raycynix.Extensions.Observability.AspNetCore`.
`AddRaycynixAspNetCoreObservability(...)` already calls `AddRaycynixObservability()` for you.
