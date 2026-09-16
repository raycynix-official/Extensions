# Raycynix.Extensions.Tracing.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

This package contains the shared identity and standard activity source used by Raycynix instrumentation.

## API

- `RaycynixTracing.SourceName` — the stable source name `Raycynix.Extensions`
- `RaycynixTracing.SourceVersion` — the instrumentation release version
- `RaycynixTracing.ActivitySource` — the shared `System.Diagnostics.ActivitySource`

```csharp
using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;

using var activity = RaycynixTracing.ActivitySource.StartActivity(
    "checkout.run",
    ActivityKind.Internal);

activity?.SetTag("raycynix.workflow", "checkout");
```

The 2.x `ITracer` contract was removed. Consumers now use `System.Diagnostics.Activity` directly, allowing any compatible listener or OpenTelemetry exporter to collect the same spans.
