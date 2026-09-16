# Raycynix.Extensions.Messaging.Grpc

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging.Grpc` contains the direct gRPC transport for Raycynix messaging.

## What it contains

- `AddGrpc(...)`
- `AddGrpcUnary<TGrpcClient, TRequest, TResponse>(...)`
- `GrpcDirectMessagingConfiguration`
- `IGrpcRequestClient`
- gRPC unary request/response transport over `Grpc.Net.Client`

## Usage

Example `appsettings.json`:

```json
{
  "GrpcDirectMessagingConfiguration": {
    "Address": "https://catalog-service"
  }
}
```

Register the transport and map logical destinations to unary gRPC calls:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddGrpc(builder.Configuration)
    .AddGrpcUnary<Catalog.CatalogClient, GetCatalogItemRequest, CatalogItemReply>(
        "catalog/get-item",
        static async (client, request, cancellationToken) =>
            await client.GetItemAsync(request, cancellationToken: cancellationToken));
```

Send a direct request:

```csharp
public class CatalogGateway(
    IRequestEnvelopeFactory envelopeFactory,
    IGrpcRequestClient requestClient)
{
    public async Task<CatalogItemReply> GetItemAsync(string sku, CancellationToken cancellationToken)
    {
        var request = envelopeFactory.Create(
            new GetCatalogItemRequest { Sku = sku },
            destination: "catalog/get-item",
            format: MessageFormat.Grpc);

        var response = await requestClient.SendAsync<GetCatalogItemRequest, CatalogItemReply>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```

Notes:

- `destination` is a logical operation key, not a URL
- the registered unary mapping decides which generated gRPC client and method are called
- contract, correlation, trace, and security headers are added on the shared request envelope level

## Logging

The gRPC transport uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover request operation lookup, client sends, processor execution, mapped RPC statuses, and processing failures. Request/response payloads and header values are not logged.

## Migrating From 2.x

Version 3.0 targets .NET 10 and depends on `Raycynix.Extensions.Messaging` 3.0. Existing registration and request APIs remain compatible.
