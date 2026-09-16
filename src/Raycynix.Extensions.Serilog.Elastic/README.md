# Raycynix.Extensions.Serilog.Elastic

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Serilog.Elastic` connects the Raycynix Serilog pipeline to
Elasticsearch or Elastic Cloud through the officially supported
`Elastic.Serilog.Sinks` package. Events are written as ECS-compatible documents
to Elasticsearch data streams.

## Compatibility

- .NET 10
- `Raycynix.Extensions.Serilog` at the same shared release version
- `Elastic.Serilog.Sinks` 9.0.0
- Elastic Stack 8.15.0 or later, as required by the official sink 9.0.0

The package intentionally relies on the dependency versions selected by the
official sink. Applications should not override its Elastic ingest or transport
dependencies independently unless the resulting graph has been runtime-tested.

## Installation

```bash
dotnet add package Raycynix.Extensions.Serilog.Elastic
```

## Registration

```csharp
using Raycynix.Extensions.Serilog;
using Raycynix.Extensions.Serilog.Elastic;

var builder = Host.CreateApplicationBuilder(args);

builder.AddRaycynixSerilog(logging =>
{
    logging.AddElastic();
});
```

`AddElastic()` contributes an output sink, so the core fallback console sink is
not added. Configure a native console sink explicitly if both destinations are
required.

## Direct Elasticsearch connection

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "Enabled": true,
        "ConnectionMode": "Elasticsearch",
        "Nodes": [
          "https://elastic-01.example.com:9200",
          "https://elastic-02.example.com:9200"
        ],
        "UseSniffing": false,
        "BootstrapMethod": "Silent",
        "Authentication": {
          "Mode": "ApiKey",
          "ApiKey": "replace-from-secret-provider"
        },
        "DataStream": {
          "Type": "logs",
          "Dataset": "orders",
          "Namespace": "production"
        }
      }
    }
  }
}
```

At least one absolute HTTP or HTTPS node URI is required in `Elasticsearch`
connection mode.

## Elastic Cloud connection

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "ConnectionMode": "ElasticCloud",
        "CloudId": "deployment:encoded-value",
        "Authentication": {
          "Mode": "ApiKey",
          "ApiKey": "replace-from-secret-provider"
        }
      }
    }
  }
}
```

Elastic Cloud requires a Cloud ID and either API key or basic authentication.
Node sniffing is rejected in Cloud mode.

## Authentication

| Mode | Required values | Notes |
|---|---|---|
| `None` | None | Supported for direct Elasticsearch connections only |
| `ApiKey` | `ApiKey` | Accepts the encoded Elastic API key |
| `Basic` | `Username`, `Password` | Supported for direct nodes and Elastic Cloud |

Secrets should be supplied through environment variables, user secrets, Vault,
Kubernetes Secrets, or another configuration provider. Do not commit them to
`appsettings.json`.

```text
Raycynix__Serilog__Elastic__Authentication__ApiKey=encoded-key
```

## ECS data stream naming

The data stream name is composed as `<type>-<dataset>-<namespace>`.

| Component | Default |
|---|---|
| `Type` | `logs` |
| `Dataset` | Normalized Raycynix `ServiceName` |
| `Namespace` | Normalized Raycynix `Environment` |

For example, `Orders.Api` in `QA West` produces:

```text
logs-orders_api-qa_west
```

`DataStream:IlmPolicy` can select an optional index lifecycle management
policy. `BootstrapMethod` controls installation of component and index
templates and supports the official sink values `None`, `Silent`, and
`Failure`.

## Sink filtering and ECS content

`MinimumLevel` filters only the Elastic destination. It does not change the
global Serilog minimum level.

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "MinimumLevel": "Warning",
        "IncludeHost": true,
        "IncludeProcess": true,
        "IncludeUser": false,
        "IncludeActivity": true,
        "FilterProperties": [
          "SensitivePayload"
        ]
      }
    }
  }
}
```

## Channel and backpressure configuration

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "Buffer": {
          "ExportMaxRetries": 3,
          "ExportMaxConcurrency": 4,
          "InboundBufferMaxSize": 10000,
          "OutboundBufferMaxSize": 1000,
          "OutboundBufferMaxLifetime": "00:00:05",
          "FullMode": "Wait"
        }
      }
    }
  }
}
```

All buffer values are optional. Omitted values preserve the official sink
defaults. Sizes and concurrency must be positive; retry count cannot be
negative; buffer lifetime must be greater than zero.

The official Elastic sink is buffered but not durable. If lossless delivery is
required, use a durable local destination and an external shipping agent.

## Proxy and TLS

```json
{
  "Raycynix": {
    "Serilog": {
      "Elastic": {
        "CertificateFingerprint": "sha256-fingerprint",
        "Proxy": {
          "Url": "http://proxy.example.com:8080",
          "Username": "proxy-user",
          "Password": "replace-from-secret-provider"
        }
      }
    }
  }
}
```

Proxy username and password must be supplied together. `DebugMode` enables the
official Elastic transport diagnostics and should be used deliberately because
debug output can be verbose.

## Programmatic configuration

Configuration values can be overridden in code:

```csharp
builder.AddRaycynixSerilog(logging =>
{
    logging.AddElastic(options =>
    {
        options.Nodes.Add(new Uri("https://localhost:9200"));
        options.MinimumLevel = LogEventLevel.Warning;
        options.Order = 1_000;
    });
});
```

Advanced integrations can use `ConfigureSinkOptions` and
`ConfigureTransport`. These callbacks run after the Raycynix settings have been
applied and can replace official sink or transport behavior.

## Option reference

| Option | Default | Purpose |
|---|---|---|
| `Enabled` | `true` | Enables the Elastic sink |
| `ConnectionMode` | `Elasticsearch` | Selects direct nodes or Elastic Cloud |
| `Nodes` | Empty | Direct Elasticsearch node endpoints |
| `UseSniffing` | `false` | Enables node discovery for direct connections |
| `CloudId` | `null` | Elastic Cloud deployment identifier |
| `Authentication` | `None` | Transport authentication settings |
| `DataStream` | `logs-<service>-<environment>` | ECS data stream and optional ILM settings |
| `BootstrapMethod` | `Silent` | Official template bootstrap behavior |
| `MinimumLevel` | `Information` | Elastic sink-specific minimum level |
| `IncludeHost` | `true` | Includes ECS host fields |
| `IncludeProcess` | `true` | Includes ECS process fields |
| `IncludeUser` | `false` | Includes ECS user fields |
| `IncludeActivity` | `true` | Includes Activity trace and span data |
| `FilterProperties` | Empty | Excludes selected Serilog properties from ECS output |
| `CertificateFingerprint` | `null` | Pins the expected server certificate fingerprint |
| `Proxy` | Empty | Configures proxy URI and optional credentials |
| `Buffer` | Official defaults | Configures channel concurrency, capacity, lifetime, retries, and full mode |
| `DebugMode` | `false` | Enables Elastic transport debug mode |
| `Order` | `1000` | Controls execution relative to other Raycynix configurators |

## Validation and failure behavior

Invalid connection, authentication, proxy, data stream, or buffer settings fail
during `AddElastic()` registration. When `Enabled` is `false`, connection
validation is skipped and the core fallback console sink remains eligible.

Register the integration only once per Raycynix Serilog builder. Duplicate
registration throws `InvalidOperationException`.
