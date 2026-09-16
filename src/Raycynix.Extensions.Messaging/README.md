# Raycynix.Extensions.Messaging

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging` contains the transport-agnostic messaging foundation for Raycynix applications.

## What it contains

- `AddRaycynixMessaging(...)`
- `MessagingBuilder`
- `MessagingConfiguration`
- `JsonMessagingConfiguration`
- `GrpcMessagingConfiguration`
- `IMessageSerializer`
- `IMessageEnvelopeFactory`
- `IRequestEnvelopeFactory`
- `IDirectRequestClient`
- `IMessageCodec`
- `IMessageCodecResolver`
- `IMessagePublisher`
- `IMessageHandler<TMessage>`
- `MessageEnvelope<TMessage>`
- `RequestEnvelope<TRequest>`
- `ResponseEnvelope<TResponse>`
- `SerializedMessage`
- built-in JSON serialization through `Newtonsoft.Json`
- delegate-based gRPC/protobuf codec registration through `AddGrpcMessage<TMessage>(...)`
- transport-neutral message envelope creation and serialization
- transport-neutral direct request/response abstractions
- incoming dispatch pipeline with retry, deduplication, and idempotency foundations
- optional `System.Diagnostics.Metrics` instrumentation when `IMeterFactory` is registered
- optional Microsoft `ILogger<T>` diagnostics for publish, dispatch, incoming processing, and outbox recovery
- scoped envelope/request factories that can project ambient security context safely
- in-memory inbox/outbox and outbox recovery foundation with dispatch leases

## What it does not contain

- broker-specific Kafka client setup
- broker-specific RabbitMQ client setup
- persistent inbox/outbox storage
- database-backed transactional coordination
- broker topology management beyond provider packages

## Usage

Example `appsettings.json`:

```json
{
  "MessagingConfiguration": {
    "SourceName": "orders-service",
    "DefaultFormat": "Json",
    "Outbox": {
      "Enabled": true,
      "EnableRecovery": true,
      "AutoDispatchOnPublish": false
    },
    "IncomingProcessing": {
      "TrustedSources": [
        "orders.service"
      ]
    }
  }
}
```

Register the base package and optional codecs:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration, options =>
{
    options.DefaultFormat = MessageFormat.Json;
})
.AddGrpcMessage<MyGrpcMessage>(
    message => message.ToByteArray(),
    payload => MyGrpcMessage.Parser.ParseFrom(payload.Span));
```

Register direct request handlers in the shared pipeline:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddRequestHandler<GetOrderRequest, GetOrderResponse, GetOrderRequestHandler>("orders.v1/get");
```

Create and publish a message through a broker transport:

```csharp
public class OrderService(
    IMessageEnvelopeFactory envelopeFactory,
    IMessagePublisher messagePublisher)
{
    public async Task PublishOrderCreatedAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var envelope = envelopeFactory.Create(
            message,
            destination: "orders.created",
            format: MessageFormat.Json);

        await messagePublisher.PublishAsync(envelope, cancellationToken);
    }
}
```

Create and send a direct request through `HttpJson` or `Grpc` transport:

```csharp
public class CatalogGateway(
    IRequestEnvelopeFactory envelopeFactory,
    IDirectRequestClient directRequestClient)
{
    public async Task<CatalogItemResponse> GetAsync(string sku, CancellationToken cancellationToken)
    {
        var request = envelopeFactory.Create(
            new CatalogItemRequest(sku),
            destination: "catalog/get-item",
            format: MessageFormat.Json);

        var response = await directRequestClient.SendAsync<CatalogItemRequest, CatalogItemResponse>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```

Register and dispatch incoming handlers:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddMessageHandler<OrderCreatedMessage, OrderCreatedHandler>();

public sealed class OrderConsumer(IMessageDispatcher dispatcher)
{
    public async Task ConsumeAsync(OrderCreatedMessage message, CancellationToken cancellationToken)
    {
        var envelope = new MessageEnvelope<OrderCreatedMessage>
        {
            Message = message,
            Destination = "orders.created",
            Format = MessageFormat.Json,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await dispatcher.DispatchAsync(envelope, cancellationToken);
    }
}
```

Contract metadata and propagation headers are added automatically:

- `X-Contract-Name`
- `X-Contract-Version`
- `X-Correlation-Id`
- `X-Message-Source`
- `traceparent`
- service identity headers when `ISecurityContext` is available

For a concrete transport, add one of the provider packages:

- `Raycynix.Extensions.Messaging.Kafka`
- `Raycynix.Extensions.Messaging.RabbitMQ`
- `Raycynix.Extensions.Messaging.HttpJson`
- `Raycynix.Extensions.Messaging.Grpc`

The base package also includes:

- inbound security-header validation
- scoped inbound `ISecurityContext` projection from messaging headers
- declarative handler authorization using shared security attributes
- background outbox recovery service for in-memory recovery scenarios
- dispatch leasing to prevent duplicate outbox recovery publishes

If you need durable inbox and outbox storage instead of the built-in in-memory implementation, add `Raycynix.Extensions.Messaging.Database` on top of the shared database infrastructure.

## Logging

The package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover message publishing decisions, dispatch attempts, retry decisions, incoming processing, inbox/outbox decisions, and outbox recovery cycles. Message payloads, header values, security header values, and serialized content are not logged.
