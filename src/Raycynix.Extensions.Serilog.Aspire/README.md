# Raycynix.Extensions.Serilog.Aspire

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Serilog.Aspire` adds the Raycynix Serilog pipeline to an
Aspire AppHost through `IDistributedApplicationBuilder`.

## Compatibility

- .NET 10
- Aspire.Hosting 13.5.4
- `Raycynix.Extensions.Serilog` at the same shared release version

## Installation

```bash
dotnet add package Raycynix.Extensions.Serilog.Aspire
```

## AppHost registration

```csharp
using Raycynix.Extensions.Serilog.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddRaycynixSerilog();

builder.AddProject<Projects.Api>("api");
builder.AddProject<Projects.Worker>("worker");

builder.Build().Run();
```

The extension returns the same `IDistributedApplicationBuilder`, so normal
Aspire resource registration can continue fluently.

## What the package configures

The package configures the logging pipeline of the AppHost process itself. It
uses the same behavior as the core package:

- standard Microsoft `ILogger<T>` integration;
- native `Serilog` configuration;
- `Raycynix:Serilog` metadata conventions;
- dependency-injected Serilog components;
- fallback console output;
- ordered logger and sink configurators.

Aspire resources are separate processes. AppHost service registration cannot
modify the DI container of an orchestrated API, worker, executable, or
container.

## Configure every .NET resource

Each orchestrated .NET project should reference
`Raycynix.Extensions.Serilog` and register it in its own host:

```csharp
using Raycynix.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixSerilog();

var app = builder.Build();
app.Run();
```

For solutions with a shared ServiceDefaults project, place the core
registration in the shared host-builder extension used by each service. Keep
the Aspire package in AppHost only.

## AppHost configuration

The AppHost reads the same sections as the core package:

```json
{
  "Raycynix": {
    "Serilog": {
      "ServiceName": "commerce-apphost",
      "Environment": "Development"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Aspire": "Information",
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      }
    ]
  }
}
```

If `Serilog:WriteTo` is omitted, the core fallback console sink is used.

## Configuration in code

The overload exposes the standard `RaycynixSerilogBuilder` callback:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.Options.ServiceName = "commerce-apphost";
    logging.Options.ApplyDefaultLevelOverrides = false;

    logging.ConfigureLogger((context, configuration) =>
        configuration.Enrich.WithProperty("Orchestrator", "Aspire"));
});
```

## Optional sinks

Optional Raycynix sink packages compose through the same callback. For example,
after installing `Raycynix.Extensions.Serilog.Elastic`:

```csharp
using Raycynix.Extensions.Serilog.Aspire;
using Raycynix.Extensions.Serilog.Elastic;

builder.AddRaycynixSerilog(logging =>
{
    logging.AddElastic();
});
```

This adds Elastic output to AppHost only. Resource projects require their own
sink registration and configuration.

## Registration rules

Call `AddRaycynixSerilog()` once for the AppHost service collection. Duplicate
registration throws `InvalidOperationException`, matching the core package.

The Aspire package deliberately contains no resource annotations and does not
inject logging configuration into child resources. This keeps secrets,
environment-specific sink choices, and resource logging lifecycles explicit.
