# Raycynix.Extensions.Database.Observability

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

Optional tracing and metrics integration for Raycynix database infrastructure operations.

## What It Provides

- `AddObservability()`
- an `IDatabaseObservability` implementation backed by Raycynix tracing and `System.Diagnostics.Metrics`
- operation counters with named provider, operation, and status tags
- duration histograms for observed database operations
- trace tags for provider and operation names

Without this package, `Raycynix.Extensions.Database` uses a no-op observability implementation.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddObservability();
```

Register the core database package and exactly one provider before enabling observability.

## Observed Operations

The package observes infrastructure operations such as:

- database initialization
- `EnsureCreated`
- EF Core migrations
- model creation

Metrics use the `raycynix.database.*` prefix and the shared `Raycynix.Extensions` meter. Register `IMeterFactory` through `AddRaycynixMetrics()` or the observability packages to enable them.

## Logging

The observability package emits optional `Microsoft.Extensions.Logging` diagnostics for observability setup and operation recording. It logs provider names, operation names, and statuses, but does not log connection strings or credentials.
