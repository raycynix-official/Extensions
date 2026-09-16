# Raycynix.Extensions.Database.MsSql

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

SQL Server provider integration for `Raycynix.Extensions.Database`.

## What It Provides

- `AddMsSql(...)`
- `MsSqlServerOptions`
- SQL Server structured connection-string composition
- SQL Server provider-specific validation
- EF Core `UseSqlServer(...)` configuration with retries, command timeout, and migrations assembly support

The provider is selected by calling `.AddMsSql(...)`.

## Usage

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddMsSql(sqlServer =>
    {
        sqlServer.TrustServerCertificate = false;
        sqlServer.CommandTimeoutSeconds = 30;
    });
```

## Configuration

Schema configuration is optional. `.AddMsSql()` preserves SQL Server's normal schema resolution.
To set an application-wide default, use `.AddMsSql(options => options.DefaultSchema = "app")`
or `DatabaseOptions:MsSqlServerOptions:DefaultSchema` in configuration. The default is `null`;
empty or whitespace names are rejected.

For individual entities, apply `[DatabaseSchema("sales")]` to a `GenericConfigurator<T>`
or call `ConfigureEntity(modelBuilder).EntitySchema("sales")`.
Precedence is fluent entity mapping, then the attribute, then `DefaultSchema`, then the
provider/database default. `EntitySchema(null)` restores the model/provider default.

The default schema is captured when the provider model configurator is first resolved and
automatically participates in runtime and design-time model cache keys. Restart the application
to change this application-wide setting. Custom configurators that select schemas at runtime
must include those values in `GetModelShapeCacheKey()`.

Identity and messaging Inbox/Outbox tables inherit this default unless explicitly mapped otherwise.
`__EFMigrationsHistory` keeps its standard provider location; this option does not relocate it.

### Connection settings

```json
{
  "DatabaseOptions": {
    "ConnectionOptions": {
      "Host": "localhost",
      "Name": "app",
      "Username": "sa",
      "Password": "secret"
    },
    "UseMigrations": true,
    "EnsureCreated": false,
    "MsSqlServerOptions": {
      "TrustServerCertificate": false,
      "MultipleActiveResultSets": false,
      "CommandTimeoutSeconds": 30
    }
  }
}
```

When a raw `ConnectionString` is not supplied, structured SQL Server configuration requires `Host` and `Name`. Username and password are passed through when provided.

## Logging

The provider emits optional `Microsoft.Extensions.Logging` diagnostics for validation, connection-string source selection, and EF Core provider configuration. Connection strings, usernames, and passwords are never logged.

## Migrating From 2.x

- Replace `MsSqlServerConfiguration` with `MsSqlServerOptions`.
- Import it from `Raycynix.Extensions.Database.MsSql.Options`.
- Rename the section from `DatabaseConfiguration:MsSqlServerConfiguration` to `DatabaseOptions:MsSqlServerOptions`.
- Negative command timeouts now fail during options validation.
