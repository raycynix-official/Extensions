# Raycynix.Extensions.Common

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Common` contains shared primitives and helper utilities used across Raycynix extension packages.

## What it contains

- `IOperationContext` and `OperationContext`
- assembly metadata helpers
- reusable disposable helpers

## Usage

```csharp
services.TryAddScoped<IOperationContext, OperationContext>();

var serviceName = AssemblyHelper.CurrentName();
var serviceVersion = AssemblyHelper.CurrentVersion();

using var _ = NoopDisposable.Instance;
```

`OperationContext.CorrelationId` and the fallback `TraceId` are generated lazily and remain stable for the same context instance. When `Activity.Current` exists, `TraceId` follows the active diagnostic activity.

## Migrating From 2.x

Version 3.0 targets .NET 10. Existing public APIs remain compatible; update this package together with the other Raycynix 3.0 packages.
