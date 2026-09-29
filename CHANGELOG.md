# Changelog

All published packages share version **3.1.0**, defined in [Directory.Build.props](Directory.Build.props).

## 3.1.0

### Added

- Multiple Raycynix database contexts with shared database/provider settings, named context overrides, isolated models, typed initialization, and independent migrations history tables.

### Changed

- Host database initialization now initializes every registered context while preserving the existing single-context service surface.
- Updated all database examples to use shared options with named context overrides.

## 3.0.1

### Added

- `DatabaseSchemaAttribute` on entity configurators and `EntitySchema(...)` for fluent schema mapping, overrides, and resetting to the default.
- Optional `PostgreSqlOptions.DefaultSchema`, configured through registration or `DatabaseOptions:PostgreSqlOptions:DefaultSchema`.
- Optional `MsSqlServerOptions.DefaultSchema` with the same mapping precedence, validation, and model-cache isolation as PostgreSQL.
- Schema inheritance tests for Identity and messaging Inbox/Outbox with PostgreSQL and SQL Server; SQL generation tests documenting MySQL database qualification and SQLite schema omission.
- Provider model defaults via `IDatabaseProviderModelConfigurator`, applied before entity configurators and included in EF Core model cache keys.
- Regression coverage for schema precedence, generated PostgreSQL queries and DDL, runtime/design-time cache isolation, and registration without any schema settings.

### Changed

- Centralized all published package versions in `Directory.Build.props`; removed duplicate declarations from package projects.
- Centralized NuGet release notes with the shared version and links to release history.
- Updated dependencies, including OpenTelemetry SDK 1.18.0 and the compatible Prometheus exporter 1.18.0-beta.1 in tests and examples.
- `EntityName(...)` preserves an explicitly configured schema. Without schema configuration, database/provider defaults remain in effect; PostgreSQL uses normal `search_path` resolution. Migration history is not relocated.

### Fixed

- Fixed Prometheus endpoint HTTP 500 failures in tests caused by exporter/SDK version mismatch.
- Replaced fixed delays in RabbitMQ retry and dead-letter tests with acknowledgement completion signals.

## Package changelogs

Per-package changelogs live next to each package under `src/<PackageName>/CHANGELOG.md`.

- [Raycynix.Extensions.Common](src/Raycynix.Extensions.Common/CHANGELOG.md)
- [Raycynix.Extensions.Configuration](src/Raycynix.Extensions.Configuration/CHANGELOG.md)
- [Raycynix.Extensions.Configuration.Abstractions](src/Raycynix.Extensions.Configuration.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Configuration.AspNetCore](src/Raycynix.Extensions.Configuration.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Contracts](src/Raycynix.Extensions.Contracts/CHANGELOG.md)
- [Raycynix.Extensions.Contracts.AspNetCore](src/Raycynix.Extensions.Contracts.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Database](src/Raycynix.Extensions.Database/CHANGELOG.md)
- [Raycynix.Extensions.Database.Abstractions](src/Raycynix.Extensions.Database.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Database.AspNetCore](src/Raycynix.Extensions.Database.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Database.AspNetCore.Identity](src/Raycynix.Extensions.Database.AspNetCore.Identity/CHANGELOG.md)
- [Raycynix.Extensions.Database.Hosting](src/Raycynix.Extensions.Database.Hosting/CHANGELOG.md)
- [Raycynix.Extensions.Database.MsSql](src/Raycynix.Extensions.Database.MsSql/CHANGELOG.md)
- [Raycynix.Extensions.Database.MySql](src/Raycynix.Extensions.Database.MySql/CHANGELOG.md)
- [Raycynix.Extensions.Database.PostgreSql](src/Raycynix.Extensions.Database.PostgreSql/CHANGELOG.md)
- [Raycynix.Extensions.Database.Sqlite](src/Raycynix.Extensions.Database.Sqlite/CHANGELOG.md)
- [Raycynix.Extensions.Database.Observability](src/Raycynix.Extensions.Database.Observability/CHANGELOG.md)
- [Raycynix.Extensions.Email](src/Raycynix.Extensions.Email/CHANGELOG.md)
- [Raycynix.Extensions.Email.Abstractions](src/Raycynix.Extensions.Email.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Email.Smtp](src/Raycynix.Extensions.Email.Smtp/CHANGELOG.md)
- [Raycynix.Extensions.Exceptions](src/Raycynix.Extensions.Exceptions/CHANGELOG.md)
- [Raycynix.Extensions.Exceptions.Abstractions](src/Raycynix.Extensions.Exceptions.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Exceptions.AspNetCore](src/Raycynix.Extensions.Exceptions.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Serilog](src/Raycynix.Extensions.Serilog/CHANGELOG.md)
- [Raycynix.Extensions.Serilog.Elastic](src/Raycynix.Extensions.Serilog.Elastic/CHANGELOG.md)
- [Raycynix.Extensions.Serilog.Aspire](src/Raycynix.Extensions.Serilog.Aspire/CHANGELOG.md)
- [Raycynix.Extensions.Messaging](src/Raycynix.Extensions.Messaging/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.Abstractions](src/Raycynix.Extensions.Messaging.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.Database](src/Raycynix.Extensions.Messaging.Database/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.Grpc](src/Raycynix.Extensions.Messaging.Grpc/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.HttpJson](src/Raycynix.Extensions.Messaging.HttpJson/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.Kafka](src/Raycynix.Extensions.Messaging.Kafka/CHANGELOG.md)
- [Raycynix.Extensions.Messaging.RabbitMQ](src/Raycynix.Extensions.Messaging.RabbitMQ/CHANGELOG.md)
- [Raycynix.Extensions.Metrics](src/Raycynix.Extensions.Metrics/CHANGELOG.md)
- [Raycynix.Extensions.Metrics.Abstractions](src/Raycynix.Extensions.Metrics.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Metrics.AspNetCore](src/Raycynix.Extensions.Metrics.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Observability](src/Raycynix.Extensions.Observability/CHANGELOG.md)
- [Raycynix.Extensions.Observability.AspNetCore](src/Raycynix.Extensions.Observability.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Secrets](src/Raycynix.Extensions.Secrets/CHANGELOG.md)
- [Raycynix.Extensions.Security](src/Raycynix.Extensions.Security/CHANGELOG.md)
- [Raycynix.Extensions.Security.Abstractions](src/Raycynix.Extensions.Security.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Security.AspNetCore](src/Raycynix.Extensions.Security.AspNetCore/CHANGELOG.md)
- [Raycynix.Extensions.Tracing](src/Raycynix.Extensions.Tracing/CHANGELOG.md)
- [Raycynix.Extensions.Tracing.Abstractions](src/Raycynix.Extensions.Tracing.Abstractions/CHANGELOG.md)
- [Raycynix.Extensions.Tracing.AspNetCore](src/Raycynix.Extensions.Tracing.AspNetCore/CHANGELOG.md)
