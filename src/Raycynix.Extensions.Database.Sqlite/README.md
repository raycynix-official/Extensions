# Raycynix.Extensions.Database.Sqlite

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

SQLite provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddSqlite(...)`
- `SqliteOptions`
- SQLite structured connection-string composition
- SQLite provider-specific validation
- EF Core `UseSqlite(...)` configuration with command timeout and migrations assembly support

The provider is selected by calling `.AddSqlite(...)`.

## Usage

### Schema behavior

EF Core's SQLite provider does not support relational schemas. Explicit schema metadata from
`DatabaseSchema` or `EntitySchema` is ignored in generated queries and table-creation SQL;
for example, `sales.orders` is emitted as the unqualified table `orders`.
There is no `SqliteOptions.DefaultSchema` option. Use `.AddSqlite()` without schema configuration.
Different schema names do not isolate tables with the same name in SQLite.

### Registration

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.EnsureCreated = true;
        options.UseMigrations = false;
    })
    .AddSqlite(sqlite =>
    {
        sqlite.CommandTimeoutSeconds = 30;
    });
```

## Configuration

```json
{
  "DatabaseOptions": {
    "ConnectionOptions": {
      "Name": "app.db"
    },
    "EnsureCreated": true,
    "UseMigrations": false,
    "SqliteOptions": {
      "Mode": "ReadWriteCreate",
      "Cache": "Shared",
      "CommandTimeoutSeconds": 30
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured SQLite configuration requires only `Name`, which becomes the SQLite data source.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings and data source values are never logged.

## Migrating From 2.x

- Replace `SqliteConfiguration` with `SqliteOptions`.
- Import it from `Raycynix.Extensions.Database.Sqlite.Options`.
- Rename the section from `DatabaseConfiguration:SqliteConfiguration` to `DatabaseOptions:SqliteOptions`.
- Invalid mode, cache, and negative command timeout values now fail during options validation.
