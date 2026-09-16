# Raycynix.Extensions.Database.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

ASP.NET Core startup integration for Raycynix database initialization.

## What It Provides

- `InitializeRaycynixDatabaseAsync(this WebApplication app)`

The package delegates initialization to `Raycynix.Extensions.Database.Hosting` and returns the same `WebApplication` instance for chaining.

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql();

var app = builder.Build();

await app.InitializeRaycynixDatabaseAsync();

app.Run();
```

Register `Raycynix.Extensions.Database`, exactly one provider package, and any required model assemblies before calling the initializer.

## Configuration

```json
{
  "DatabaseOptions": {
    "ConnectionString": "Host=localhost;Port=5432;Database=app;Username=app;Password=secret",
    "UseMigrations": true,
    "EnsureCreated": false,
    "PostgreSqlOptions": {
      "CommandTimeoutSeconds": 30,
      "IncludeErrorDetail": false
    }
  }
}
```
