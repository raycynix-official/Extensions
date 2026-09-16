# Raycynix.Extensions.Messaging.Database

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Messaging.Database` adds persistent inbox and outbox storage for Raycynix messaging on top of `Raycynix.Extensions.Database`.

## Upgrading from 2.x

Version 3 stores `CreatedAt`, `UpdatedAt`, and `NextAttemptAt` persistence columns as signed 64-bit UTC tick values. Existing deployments must migrate those timestamp columns before starting version 3. The change enables indexed filtering, ordering, and bounded cleanup directly in SQL on every supported provider.

## What it contains

- `AddDatabasePersistence(...)`
- persistent `IIncomingMessageInboxStore`
- persistent `IMessageOutboxStore`
- EF Core configurators registered into the shared `DatabaseContext`
- ambient unit-of-work aware outbox persistence for shared `DatabaseContext` scopes
- optional background retention cleanup for inbox/outbox tables
- optional Microsoft `ILogger<T>` diagnostics for persistence, leases, and cleanup

## What it does not contain

- broker-specific Kafka client setup
- broker-specific RabbitMQ client setup
- EF Core entities for your application domain
- cross-resource distributed transactions

## Usage

Example `appsettings.json`:

```json
{
  "MessagingDatabasePersistenceConfiguration": {
    "InboxTableName": "messaging_inbox",
    "OutboxTableName": "messaging_outbox",
    "EnableCleanup": true,
    "CleanupInterval": "00:05:00",
    "CleanupBatchSize": 500,
    "ProcessedInboxRetention": "3.00:00:00",
    "DispatchedOutboxRetention": "3.00:00:00"
  }
}
```

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql();

builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddDatabasePersistence(builder.Configuration);
```

You can still override specific values in code:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddDatabasePersistence(builder.Configuration, options =>
    {
        options.InboxTableName = "tenant_a_messaging_inbox";
    });
```

The package replaces the default in-memory inbox/outbox stores with database-backed implementations and registers its EF Core configurators into the shared `DatabaseContext` through `AddRaycynixDatabaseAssembly(...)`. Table creation still flows through the existing Raycynix database initialization pipeline.

Inbox and outbox lease acquisition uses optimistic concurrency through EF Core model metadata, so the package stays provider-agnostic across SQLite, PostgreSQL, SQL Server, and MySQL without introducing provider-specific SQL into the messaging layer.

This package gives messaging persistence that survives process restarts, participates in the ambient shared `DatabaseContext` unit of work for outbox writes, runs retention cleanup, and works with the existing outbox recovery pipeline. It does not provide distributed transactions, but it does provide durable inbox/outbox state and database-backed recovery and dispatch leasing in the configured relational database.

## Logging

The database persistence package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover inbox/outbox status transitions, lease acquisition decisions, optimistic concurrency outcomes, and cleanup counts. Payloads, serialized headers, header values, and database connection details are not logged.

## Table Schemas

Inbox and Outbox tables inherit `PostgreSqlOptions.DefaultSchema` or `MsSqlServerOptions.DefaultSchema`
from the shared database context. Their configured table names remain unchanged. No separate schema
option is required in messaging persistence. Without a default schema, provider/database defaults apply.

MySQL treats explicitly qualified schemas as database names; SQLite ignores schema mappings.
