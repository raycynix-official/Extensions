# Raycynix.Extensions.Messaging.Kafka

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging.Kafka` contains the Kafka transport integration for Raycynix messaging.

## What it contains

- `AddKafka(...)`
- `KafkaMessagingConfiguration`
- Kafka-backed `IMessagePublisher`
- Kafka producer setup through `Confluent.Kafka`
- message publishing to Kafka topics using `MessageEnvelope<TMessage>.Destination`

## What it does not contain

- Kafka consumer hosted services
- consumer group processing pipeline
- schema registry integration
- outbox persistence
- retry topic orchestration

## Usage

Example `appsettings.json`:

```json
{
  "KafkaMessagingConfiguration": {
    "BootstrapServers": [
      "localhost:9092"
    ],
    "ClientId": "orders-service",
    "EnableIdempotence": true,
    "Acks": "all",
    "Consumer": {
      "Enabled": true,
      "Topics": [
        "orders.created"
      ]
    }
  }
}
```

Register the transport:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddKafka(builder.Configuration);
```

Publish to a Kafka topic:

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

The envelope destination is used as the Kafka topic name, and the package publishes:

- serialized payload bytes
- `message-id`, `correlation-id`, `causation-id`
- contract/version headers from the base messaging layer

## Logging

The Kafka transport uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover publish attempts, inbound consumer startup, message processing, commits, retry republishes, and dead-letter publishes. Message payloads, header values, credentials, and connection strings are not logged.
