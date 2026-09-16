# Raycynix.Extensions.Serilog

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Serilog` is a thin, opinionated integration layer over Serilog for .NET hosted applications.

The package does not replace `Microsoft.Extensions.Logging` and does not introduce a custom logger abstraction.
Application services continue to consume the standard `ILogger<T>` and `ILoggerFactory` interfaces.

## Features

* One-line Serilog registration
* Support for modern and classic .NET hosting models
* Native `Microsoft.Extensions.Logging` integration
* Native Serilog configuration support
* Dependency-injected sinks, enrichers, filters, and destructuring policies
* Standard Raycynix service metadata
* Automatic fallback console sink
* Ordered extension pipeline
* Support for optional integration packages
* Programmatic Serilog configuration escape hatch

## Package family

| Package | Purpose |
|---|---|
| `Raycynix.Extensions.Serilog` | Core hosting integration, conventions, native Serilog configuration, DI discovery, and fallback console output |
| `Raycynix.Extensions.Serilog.Elastic` | Official Elastic sink integration with validated ECS data stream, connection, authentication, and channel options |
| `Raycynix.Extensions.Serilog.Aspire` | One-line Serilog registration for an Aspire AppHost process |

## Supported application models

| Application model                     | Registration API                                 |
|---------------------------------------|--------------------------------------------------|
| `HostApplicationBuilder`              | `AddRaycynixSerilog()`                           |
| `WebApplicationBuilder`               | `AddRaycynixSerilog()`                           |
| Classic `IHostBuilder`                | `UseRaycynixSerilog()`                           |
| Custom `IServiceCollection` bootstrap | `AddRaycynixSerilog(configuration, environment)` |
| Worker Service                        | Modern or classic host registration              |
| ASP.NET Core                          | `AddRaycynixSerilog()`                           |
| Aspire AppHost                        | `Raycynix.Extensions.Serilog.Aspire` package     |

`WebApplicationBuilder` implements `IHostApplicationBuilder`, so a separate ASP.NET Core registration method is not
required.

## Installation

```bash
dotnet add package Raycynix.Extensions.Serilog
```

## Compatibility

The 3.x package line targets .NET 10 and the Microsoft.Extensions 10 hosting stack. It
uses Serilog 4.3, Serilog.Extensions.Hosting 10, and
Serilog.Settings.Configuration 10. Applications write through the standard
Microsoft `ILogger<T>` API; direct use of the static Serilog logger is optional.

## Minimal configuration

The registration call is sufficient. If no sink is supplied, a console sink is
added automatically:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.AddRaycynixSerilog();
```

Application settings can then separate Raycynix conventions from native
Serilog behavior:

```json
{
  "Raycynix": {
    "Serilog": {
      "ServiceName": "orders-api"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

## Modern host registration

Use `AddRaycynixSerilog()` with `HostApplicationBuilder`:

```csharp
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRaycynixSerilog();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
```

## ASP.NET Core registration

The same method works with `WebApplicationBuilder`:

```csharp
using Raycynix.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixSerilog();

var app = builder.Build();

app.MapGet("/", (
    ILogger<Program> logger) =>
{
    logger.LogInformation("Handling root endpoint");

    return Results.Ok();
});

await app.RunAsync();
```

## Classic `IHostBuilder` registration

Use `UseRaycynixSerilog()` with the classic generic host:

```csharp
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog;

var host = Host
    .CreateDefaultBuilder(args)
    .UseRaycynixSerilog()
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
```

The Raycynix configuration callback executes while the host configures its service collection.

```csharp
var host = Host
    .CreateDefaultBuilder(args)
    .UseRaycynixSerilog(logging =>
    {
        logging.Options.ServiceName = "orders-worker";
        logging.Options.ServiceVersion = "1.2.0";
    })
    .Build();
```

## Direct `IServiceCollection` registration

Custom hosting models, integration tests, and manually managed service collections can register the package directly:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Serilog;

IConfiguration configuration =
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

IHostEnvironment environment = new CustomHostEnvironment
    {
        ApplicationName = "orders-tests",
        EnvironmentName = Environments.Development
    };

var services = new ServiceCollection();

services.AddRaycynixSerilog(configuration, environment);
```

This overload requires an `IHostEnvironment` because service name and deployment environment defaults are resolved from
hosting metadata.

## Application logging

Application code uses the standard Microsoft logger:

```csharp
using Microsoft.Extensions.Logging;

public sealed class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public void Process(string orderId)
    {
        logger.LogInformation(
            "Processing order {OrderId}",
            orderId);
    }
}
```

Structured message templates are passed through the official Serilog Microsoft logging provider.

## Dynamic logger categories

Use `ILoggerFactory` when the category is selected dynamically:

```csharp
public sealed class StartupService
{
    private readonly ILogger _logger;

    public StartupService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Raycynix.Startup");
    }

    public void Start()
    {
        _logger.LogInformation("Application startup completed");
    }
}
```

## Raycynix configuration

Raycynix-specific conventions are configured under `Raycynix:Serilog`:

```json
{
  "Raycynix": {
    "Serilog": {
      "ServiceName": "orders-api",
      "ServiceVersion": "1.0.0",
      "Environment": "Production",
      "SerilogSectionName": "Serilog",
      "ApplyDefaultLevelOverrides": true,
      "UseDefaultConsoleWhenNoSinksConfigured": true,
      "PreserveStaticLogger": false,
      "WriteToProviders": false
    }
  }
}
```

When the following values are omitted, the package resolves them automatically:

* `ServiceName` from `IHostEnvironment.ApplicationName`
* `ServiceVersion` from the entry assembly informational version
* `Environment` from `IHostEnvironment.EnvironmentName`

The resolved snapshot is registered directly as `RaycynixSerilogOptions`:

```csharp
public sealed class Worker(RaycynixSerilogOptions loggingOptions)
{
}
```

The package does not register a separate `IOptions<RaycynixSerilogOptions>`
pipeline because the Serilog logger is constructed once with the resolved
startup snapshot.

### Raycynix option reference

| Option | Default | Purpose |
|---|---|---|
| `ServiceName` | Host application name | Adds `ServiceName` to every event |
| `ServiceVersion` | Entry assembly informational version | Adds `ServiceVersion` to every event |
| `Environment` | Host environment name | Adds `Environment` to every event |
| `SerilogSectionName` | `Serilog` | Selects the native Serilog configuration section |
| `ApplyDefaultLevelOverrides` | `true` | Applies `Warning` overrides for `Microsoft` and `System` before native configuration is read |
| `UseDefaultConsoleWhenNoSinksConfigured` | `true` | Adds console output only when no other sink is registered |
| `DefaultConsoleOutputTemplate` | Raycynix structured console template | Controls only the fallback console sink |
| `PreserveStaticLogger` | `false` | Preserves an application-managed `Log.Logger` |
| `WriteToProviders` | `false` | Forwards events to other Microsoft logging providers |

The configuration callback runs after section binding and before validation,
so values supplied in code override values from configuration.

Environment variables use standard double-underscore nesting, for example:

```text
Raycynix__Serilog__ServiceName=orders-api
Serilog__MinimumLevel__Default=Debug
```

## Native Serilog configuration

Serilog-specific behavior remains in the native `Serilog` section:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
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

Use native Serilog configuration for:

* sinks
* minimum levels
* namespace overrides
* enrichers
* filters
* destructuring policies
* level switches
* output formatters
* sink-specific settings

The package does not duplicate these settings in its own configuration model.

## Fallback console sink

When no output sink is available, the package adds a default console sink.
Fallback detection includes sinks configured through `Serilog:WriteTo`,
`ReadFrom.Services()`, `ConfigureSink()`, and optional sink integrations.

The fallback can be disabled:

```json
{
  "Raycynix": {
    "Serilog": {
      "UseDefaultConsoleWhenNoSinksConfigured": false
    }
  }
}
```

The fallback output template can also be replaced:

```json
{
  "Raycynix": {
    "Serilog": {
      "DefaultConsoleOutputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    }
  }
}
```

When at least one sink is configured, the fallback console sink is not added.

## Standard Raycynix properties

The following properties are added to every Serilog event:

```text
ServiceName
ServiceVersion
Environment
```

The package also enables:

```csharp
.Enrich.FromLogContext()
```

This means properties created through standard `ILogger.BeginScope()` calls are included in Serilog events.

```csharp
using (logger.BeginScope(
           new Dictionary<string, object?>
           {
               ["CorrelationId"] = Guid.NewGuid(),
               ["Scenario"] = "Checkout"
           }))
{
    logger.LogInformation("Processing checkout");
}
```

Correlation IDs, user IDs, trace metadata, and request-specific values should normally be provided by application or
observability scopes rather than by the logging package itself.

## Configuration in code

Options and custom Serilog configuration can be applied through the builder callback:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.Options.ServiceName = "orders-api";
    logging.Options.ServiceVersion = "1.2.0";

    logging.ConfigureLogger((context, loggerConfiguration) =>
        {
            loggerConfiguration.Enrich.WithProperty(
                    "ApplicationGroup",
                    "Commerce");
        });
});
```

## Typed configurators

Optional packages and application modules can contribute to the Serilog pipeline through `IRaycynixSerilogConfigurator`.

```csharp
using Raycynix.Extensions.Serilog.Abstractions;
using Raycynix.Extensions.Serilog.Contexts;
using Serilog;

public sealed class RegionSerilogConfigurator : IRaycynixSerilogConfigurator
{
    public int Order => 100;

    public void Configure(LoggerConfiguration loggerConfiguration, RaycynixSerilogContext context)
    {
        loggerConfiguration.Enrich.WithProperty(
                "Region",
                "eu-west");
    }
}
```

Register the configurator through the Raycynix builder:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.AddConfigurator<RegionSerilogConfigurator>();
});
```

Configurators are executed in ascending `Order`.

## Inline configurators

Small application-specific changes can be registered without creating a class:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.ConfigureLogger((context, loggerConfiguration) =>
        {
            loggerConfiguration.Enrich.WithProperty(
                    "DeploymentSlot",
                    "blue");
        },
        order: 200);
});
```

When an inline callback adds a `WriteTo` destination, register it with
`ConfigureSink()` so it participates in fallback-console detection:

The following example also requires the `Serilog.Sinks.File` package.

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.ConfigureSink((context, loggerConfiguration) =>
        loggerConfiguration.WriteTo.File("logs/application.log"));
});
```

Reusable sink packages should implement
`IRaycynixSerilogSinkConfigurator` and register it through
`AddSinkConfigurator<TConfigurator>()`.

### Extension API reference

| API | Use case |
|---|---|
| `AddConfigurator<T>()` | Register a DI-created enricher, filter, destructuring, or other logger configurator |
| `AddConfigurator(instance)` | Register an existing configurator instance |
| `ConfigureLogger(...)` | Apply inline logger configuration that does not add a sink |
| `AddSinkConfigurator<T>()` | Register a typed configurator that contributes a sink |
| `ConfigureSink(...)` | Add an inline `WriteTo` destination and suppress fallback console output |
| `AddSingleton(instance)` | Make an application object available to configurators through DI |

## Dependency-injected Serilog components

The package calls `ReadFrom.Services()`, so supported Serilog components can be registered directly in dependency
injection:

```csharp
builder.Services.AddSingleton<ILogEventEnricher, UserContextEnricher>();

builder.AddRaycynixSerilog();
```

This can be used for supported:

* sinks
* enrichers
* filters
* destructuring policies
* level switches

## Optional integrations

Additional sinks should be provided through separate packages rather than included in the core package.

Example:

```csharp
using Raycynix.Extensions.Serilog;
using Raycynix.Extensions.Serilog.Elastic;

builder.AddRaycynixSerilog(logging =>
{
    logging.AddElastic();
});
```

The core package contains only the dependencies required for host integration, native Serilog configuration, and the
fallback console sink.

## Aspire AppHost

Aspire AppHost uses `IDistributedApplicationBuilder`, which has a separate
hosting contract. Install `Raycynix.Extensions.Serilog.Aspire` to configure the
AppHost logger:

```csharp
using Raycynix.Extensions.Serilog.Aspire;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddRaycynixSerilog();
```

This configures the AppHost process only. Each orchestrated .NET project must
also call the core `AddRaycynixSerilog()` method in its own process.

## Registration rules

Register Raycynix Serilog once for each service collection.

Do not combine:

```csharp
builder.AddRaycynixSerilog();
```

with:

```csharp
builder.Host.UseRaycynixSerilog();
```

for the same application.

Duplicate registration throws an `InvalidOperationException`.

## Startup and reload behavior

Raycynix metadata options are resolved and validated once during registration.
Changing `Raycynix:Serilog` after the host has started does not rebuild the
logger or replace its metadata snapshot. Native Serilog configuration retains
the behavior provided by `Serilog.Settings.Configuration`, including supported
level-switch reload scenarios.

## Troubleshooting

### Duplicate console events

Use `ConfigureSink()` for inline `WriteTo` callbacks. `ConfigureLogger()` is
intended for enrichers, filters, levels, and other non-sink configuration.
Also verify that `WriteToProviders` is not forwarding events to an equivalent
Microsoft provider.

### Missing debug or trace events

Set the native `Serilog:MinimumLevel` value. Raycynix-specific options do not
duplicate Serilog minimum-level configuration.

### Empty metadata

Inject `RaycynixSerilogOptions` directly when application code needs the
resolved metadata snapshot. Do not request `IOptions<RaycynixSerilogOptions>`.

## Static Serilog logger

By default, Serilog replaces the static `Log.Logger` with the logger created by the dependency-injection registration.

To preserve an existing static logger:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.Options.PreserveStaticLogger = true;
});
```

This option is useful when the application manages the static Serilog logger lifecycle separately.

## Other Microsoft logging providers

By default, Serilog sinks receive the events and other registered Microsoft logging providers do not.

Forwarding to Microsoft logging providers can be enabled:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.Options.WriteToProviders = true;
});
```

Enable this only when events must also be sent to providers registered through the Microsoft logging API. It may produce
duplicate output when equivalent Serilog sinks are already configured.

## Migration from `Raycynix.Extensions.Logging`

The old package exposed a custom Raycynix `ILogger<T>` implementation.

The new package uses the standard Microsoft interface.

Before:

```csharp
using Raycynix.Extensions.Logging.Abstractions;

public sealed class Worker(ILogger<Worker> logger)
{
    public void Run()
    {
        logger.Information("Worker started");
    }
}
```

After:

```csharp
using Microsoft.Extensions.Logging;

public sealed class Worker(ILogger<Worker> logger)
{
    public void Run()
    {
        logger.LogInformation("Worker started");
    }
}
```

Registration changes from separate host and service registrations to one call.

Before:

```csharp
builder.Host.UseRaycynixLogging();

builder.Services.AddRaycynixLogging(builder.Configuration);
```

After:

```csharp
builder.AddRaycynixSerilog();
```

For the classic generic host:

```csharp
Host.CreateDefaultBuilder(args)
    .UseRaycynixSerilog();
```
