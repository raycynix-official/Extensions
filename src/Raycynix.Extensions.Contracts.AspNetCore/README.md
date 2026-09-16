# Raycynix.Extensions.Contracts.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Contracts.AspNetCore` adds ASP.NET Core integration for the shared contract types defined in `Raycynix.Extensions.Contracts`.

## Package

- Target framework: `net10.0`
- Built on ASP.NET Core 10.x

## What it contains

- endpoint metadata for declared contract identities
- middleware for writing contract headers to HTTP responses
- helpers for reading contract headers from incoming requests
- minimal API helpers for contract-aware HTTP results
- MVC helpers for converting `ModelState` into `ErrorContract`
- optional Microsoft.Extensions.Logging diagnostics for contract middleware and result execution

## Installation

```csharp
builder.Services.AddRaycynixContractsAspNetCore();

var app = builder.Build();
app.UseRaycynixContracts();
```

## Response Shape Options

Use plain contract results when you want headers plus the original response body:

```csharp
app.MapGet("/catalog/prices/{id}", (HttpContext httpContext) =>
        httpContext.Contract(new Money
        {
            Amount = 149.99m,
            Currency = "USD"
        }))
    .WithContract("catalog.prices", "1.2.0");
```

Use `VersionedContract(...)` when you want the body wrapped into `VersionedContract<T>`.

## How It Works

Contract support in ASP.NET Core has two separate parts:

1. endpoint metadata, which declares the contract name and version for the HTTP endpoint
2. response shaping, which decides whether the body is a plain DTO or a `VersionedContract<T>`

When you declare a contract on an endpoint, the middleware writes these response headers automatically:

- `X-Contract-Name`
- `X-Contract-Version`

Use the result helpers only when you also want the response body to follow a contract-aware shape.

## Minimal API

Declare contract metadata directly on the endpoint:

```csharp
app.MapGet("/catalog/prices", () =>
    httpContext => httpContext.VersionedContract(
        new[] { new Money { Amount = 149.99m, Currency = "USD" } }))
    .WithContract("catalog.prices", "1.2.0");
```

`WithContract(...)` declares the endpoint contract metadata.  
`httpContext.VersionedContract(...)` reads that metadata from the current endpoint, wraps the response body into `VersionedContract<T>`, and writes the same contract headers.

Contract names must be non-empty and versions must use the `major.minor.patch` format. Version 3.0
validates this metadata while endpoints, attributes, and contract results are configured, rather than
silently producing a response without contract headers.

If you only need headers and a plain response body, return a regular ASP.NET Core result:

```csharp
app.MapGet("/catalog/prices/{id}", () =>
    TypedResults.Ok(new Money
    {
        Amount = 149.99m,
        Currency = "USD"
    }))
    .WithContract("catalog.prices", "1.2.0");
```

## MVC / Controllers

For MVC controllers or actions, use the attribute:

```csharp
[ApiController]
[Route("api/catalog/prices")]
[Contract("catalog.prices", "1.2.0")]
public sealed class CatalogController : ControllerBase
{
    [HttpGet("{id}")]
    public IResult GetPrice(string id)
    {
        var payload = new Money
        {
            Amount = 149.99m,
            Currency = "USD"
        };

        return this.VersionedContract(payload);
    }
}
```

The attribute declares endpoint metadata.  
The middleware reads that metadata and writes the headers.  
If you return `this.VersionedContract(...)`, the body is wrapped into `VersionedContract<T>` using the same endpoint metadata.

If you return `Ok(payload)` or another normal controller result, the middleware still writes headers as long as the endpoint has `[Contract(...)]`.

## Declaring vs Writing

- declare the contract on the endpoint:
  - Minimal API: `.WithContract(...)`
  - MVC: `[Contract(...)]`
- write contract headers:
  - `app.UseRaycynixContracts()`
- choose the response body format:
  - plain DTO/body: return `Ok(...)`, `TypedResults.Ok(...)`, etc.
  - versioned envelope: return `this.VersionedContract(...)` or `httpContext.VersionedContract(...)`

## Request Header Access

Read incoming contract metadata from request headers:

```csharp
if (httpContext.TryGetRequestContractMetadata(out var requestContract))
{
    // apply compatibility rules here
}
```

## Validation Errors

Convert ASP.NET Core `ModelState` into the shared transport error shape:

```csharp
var error = ModelState.ToErrorContract(traceId: HttpContext.TraceIdentifier);
```

This package intentionally provides transport integration only. It does not enforce compatibility policy or automatic contract negotiation.

## Logging

The package uses the standard `Microsoft.Extensions.Logging.ILogger<T>` abstraction when a logger is available. Logger dependencies are optional, so the package can run without registering a logging provider. It does not require `Raycynix.Extensions.Serilog`; any Microsoft-compatible logging provider can receive the events.

Contract metadata middleware and contract HTTP results write detailed execution diagnostics at `Debug`. They log contract names, versions, endpoint names, status codes, and envelope usage, but never log response payloads.

Enable Debug logs when troubleshooting contract header emission or contract result execution:

```json
{
  "Logging": {
    "LogLevel": {
      "Raycynix.Extensions.Contracts.AspNetCore": "Debug"
    }
  }
}
```
