# Raycynix.Extensions.Messaging.HttpJson

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging.HttpJson` contains the direct HTTP JSON transport for Raycynix messaging.

## What it contains

- `AddHttpJson(...)`
- `HttpJsonMessagingConfiguration`
- `IHttpJsonRequestClient`
- HTTP request/response transport over `HttpClient`
- JSON serialization through the shared `Newtonsoft.Json` codec from `Raycynix.Extensions.Messaging`

## What it does not contain

- broker integration
- background consumers
- API gateway features

## Usage

Example `appsettings.json`:

```json
{
  "HttpJsonMessagingConfiguration": {
    "BaseAddress": "https://catalog-service",
    "TimeoutSeconds": 30
  }
}
```

Register the transport:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddHttpJson(builder.Configuration);
```

Send a direct request:

```csharp
public class CatalogClient(
    IRequestEnvelopeFactory envelopeFactory,
    IHttpJsonRequestClient requestClient)
{
    public async Task<CatalogItemResponse> GetItemAsync(string sku, CancellationToken cancellationToken)
    {
        var request = envelopeFactory.Create(
            new CatalogItemRequest(sku),
            destination: "/api/catalog/items/get",
            format: MessageFormat.Json);

        var response = await requestClient.SendAsync<CatalogItemRequest, CatalogItemResponse>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```

Notes:

- `destination` is used as the relative request path
- payload serialization uses the shared `Newtonsoft.Json` codec from `Raycynix.Extensions.Messaging`
- contract, correlation, trace, and security headers are propagated automatically

## Logging

The HTTP JSON transport uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover request send/receive status, inbound processing, mapped failure statuses, and handler lookup failures. Request and response payloads, header values, and authorization data are not logged.

## Migrating From 2.x

Version 3.0 targets .NET 10 and depends on `Raycynix.Extensions.Messaging` 3.0. Existing registration and request APIs remain compatible.
