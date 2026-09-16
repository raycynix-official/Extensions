# Raycynix.Extensions.Database.MySql

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

MySQL provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddMySql(...)`
- `MySqlOptions`
- MySQL structured connection-string composition
- MySQL provider-specific validation
- EF Core `UseMySQL(...)` configuration with retries, command timeout, pooling, and migrations assembly support

The provider is selected by calling `.AddMySql(...)`.

## Usage

### Schema behavior

This package uses Oracle's `MySql.EntityFrameworkCore` provider. Generated queries and DDL
preserve an explicitly mapped schema: `EntitySchema("sales")` with `EntityName("orders")`
produces `sales.orders`. In MySQL this identifies a table in the `sales` database,
not a namespace inside the connection's selected database.

For normal single-database usage, omit `DatabaseSchema` / `EntitySchema` and select the database
through `ConnectionOptions.Name` or `Database` in the connection string. There is no separate
`MySqlOptions.DefaultSchema` option. PostgreSQL/SQL Server schema mappings have different semantics.

### Registration

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddMySql(mySql =>
    {
        mySql.Pooling = true;
        mySql.CommandTimeoutSeconds = 30;
    });
```

## Configuration

```json
{
  "DatabaseOptions": {
    "ConnectionOptions": {
      "Host": "localhost",
      "Port": 3306,
      "Name": "app",
      "Username": "app",
      "Password": "secret"
    },
    "UseMigrations": true,
    "EnsureCreated": false,
    "MySqlOptions": {
      "AllowUserVariables": true,
      "Pooling": true,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured MySQL configuration requires `Host` and `Name`.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings, usernames, and passwords are never logged.

## Migrating From 2.x

- Replace `MySqlConfiguration` with `MySqlOptions`.
- Import it from `Raycynix.Extensions.Database.MySql.Options`.
- Rename the section from `DatabaseConfiguration:MySqlConfiguration` to `DatabaseOptions:MySqlOptions`.
- Negative command timeouts now fail during options validation.
