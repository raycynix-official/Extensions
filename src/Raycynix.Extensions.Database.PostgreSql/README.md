# Raycynix.Extensions.Database.PostgreSql

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

PostgreSQL provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddPostgreSql(...)`
- `PostgreSqlOptions`
- PostgreSQL structured connection-string composition
- PostgreSQL provider-specific validation
- EF Core `UseNpgsql(...)` configuration with retries, command timeout, pooling, and migrations assembly support

The provider is selected by calling `.AddPostgreSql(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql(postgreSql =>
    {
        postgreSql.IncludeErrorDetail = false;
        postgreSql.CommandTimeoutSeconds = 30;
    });
```

## Table Schemas

Schema configuration is optional. Calling `.AddPostgreSql()` without schema options and
omitting `DatabaseSchema` / `EntitySchema` leaves table names unqualified. PostgreSQL then
uses its normal `search_path` resolution (typically the `public` schema). No schema name
is required in configuration or code.

Set a default schema for all entities through registration:

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql(options => options.DefaultSchema = "app");
```

Alternatively, set `DatabaseOptions:PostgreSqlOptions:DefaultSchema` in configuration.
The default is `null`, which preserves PostgreSQL's normal schema resolution.

Apply `DatabaseSchema` to a `GenericConfigurator<T>`, alongside the optional table-name attribute:

```csharp
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Implementations;

[DatabaseTable("orders")]
[DatabaseSchema("sales")]
public sealed class OrderConfigurator : GenericConfigurator<Order>
{
    public override Type[] DependsOn => [];
}
```

This maps `Order` to `sales.orders`. For fluent configuration, import
`Raycynix.Extensions.Database.Infrastructure` and configure the entity inside your configurator:

```csharp
public override void Configure(ModelBuilder modelBuilder)
{
    ConfigureEntity(modelBuilder)
        .EntityName("orders")
        .EntitySchema("sales");
}
```

`EntitySchema` overrides the attribute and preserves the table name. `EntityName` also preserves
the schema, so either chaining order works. These APIs live in the shared `Database.Abstractions`
and `Database` packages, which the PostgreSQL package references.

Schema precedence is fluent entity mapping, then `DatabaseSchema`, then `DefaultSchema`,
then the provider default. `DefaultSchema` applies `modelBuilder.HasDefaultSchema(...)`
before entity configurators run. Without an explicit entity schema, EF Core uses that model default;
otherwise table names remain unqualified and PostgreSQL resolves them through `search_path`.
No `public` schema is forced. Pass `null` to `EntitySchema` to reset to the default.
Empty or whitespace schema names are rejected. Use migrations or `EnsureCreated` to create
the mapped schema and tables; these mapping APIs do not execute DDL themselves.

If a schema is selected from runtime configuration, include that value in your configurator's
`GetModelShapeCacheKey()` override, just as for runtime table names.
`PostgreSqlOptions.DefaultSchema` participates in the model cache key automatically, including
design-time models. It is captured when the provider model configurator is first resolved;
restart the application to change this application-wide setting.

`DefaultSchema` only changes model mappings. The `__EFMigrationsHistory` table keeps its
standard provider location; existing migration history is not relocated. EF Core configures
that table separately through [`MigrationsHistoryTable(...)`](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/history-table).

## Configuration

```json
{
  "DatabaseOptions": {
    "ConnectionOptions": {
      "Host": "localhost",
      "Port": 5432,
      "Name": "app",
      "Username": "app",
      "Password": "secret"
    },
    "UseMigrations": true,
    "EnsureCreated": false,
    "PostgreSqlOptions": {
      "Pooling": true,
      "MinimumPoolSize": 5,
      "MaximumPoolSize": 50,
      "CommandTimeoutSeconds": 30,
      "IncludeErrorDetail": false
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured PostgreSQL configuration requires `Host`, `Name`, and `Username`.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings, usernames, and passwords are never logged.

## Migrating From 2.x

- Replace `PostgreSqlConfiguration` with `PostgreSqlOptions`.
- Import it from `Raycynix.Extensions.Database.PostgreSql.Options`.
- Rename the section from `DatabaseConfiguration:PostgreSqlConfiguration` to `DatabaseOptions:PostgreSqlOptions`.
- Invalid pool ranges and negative command timeouts now fail during options validation.
