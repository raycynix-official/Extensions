# Raycynix.Extensions.Messaging.RabbitMQ

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging.RabbitMQ` contains the RabbitMQ transport integration for Raycynix messaging.

## What it contains

- `AddRabbitMq(...)`
- `RabbitMqMessagingConfiguration`
- `RabbitMqExchangeConfiguration`
- `RabbitMqQueueConfiguration`
- `RabbitMqRetryConfiguration`
- `RabbitMqDeadLetterConfiguration`
- RabbitMQ-backed `IMessagePublisher`
- automatic exchange, queue, binding, and dead-letter topology bootstrap
- message publishing to RabbitMQ exchanges using `MessageEnvelope<TMessage>.Destination` as the routing key

## What it does not contain

- RabbitMQ consumer hosted services
- retry execution pipeline
- poison-message processing workers
- outbox persistence
- inbox/idempotency storage

## Usage

Example `appsettings.json`:

```json
{
  "RabbitMqMessagingConfiguration": {
    "Host": "localhost",
    "Port": 5672,
    "Exchange": {
      "Name": "integration.events",
      "Type": "topic"
    },
    "Queue": {
      "Name": "orders.created",
      "PrefetchCount": 16
    },
    "DeadLetter": {
      "Enabled": true,
      "Exchange": "integration.dlx",
      "Queue": "integration.dlq",
      "RoutingKey": "dead-letter"
    }
  }
}
```

Register the transport:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddRabbitMq(builder.Configuration);
```

Publish to RabbitMQ:

```csharp
public class OrderPublisher(
    IMessageEnvelopeFactory envelopeFactory,
    IMessagePublisher messagePublisher)
{
    public async Task PublishAsync(string orderId, CancellationToken cancellationToken)
    {
        var envelope = envelopeFactory.Create(
            new OrderCreatedMessage(orderId),
            destination: "orders.created",
            format: MessageFormat.Json);

        await messagePublisher.PublishAsync(envelope, cancellationToken);
    }
}
```

The package declares the configured exchange and queue on first use, binds the queue using the queue name as the routing key, and publishes messages with the envelope destination as the routing key.

Published AMQP properties include:

- `MessageId`
- `CorrelationId`
- `ContentType`
- contract/version headers from the base messaging layer

## Logging

The RabbitMQ transport uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover publish attempts, inbound consumer startup, delivery processing, acknowledgements, retry republishes, dead-letter republishes, and rejects. Message payloads, header values, credentials, and connection strings are not logged.
