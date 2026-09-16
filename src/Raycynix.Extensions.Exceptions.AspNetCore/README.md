# Raycynix.Extensions.Exceptions.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Exceptions.AspNetCore` adds ASP.NET Core middleware integration for Raycynix exceptions.

## What it contains

- `UseRaycynixExceptions(this IApplicationBuilder app)`
- `RaycynixExceptionMiddleware`
- optional Microsoft `ILogger<T>` diagnostics for handled request failures

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixExceptions();

var app = builder.Build();

app.UseRaycynixExceptions();

app.Run();
```

Mapped exceptions are written as structured JSON responses. For example, a mapped validation exception produces an HTTP response body shaped by the core exception package and the ASP.NET Core middleware.

This package depends on `Raycynix.Extensions.Exceptions` for the core exception mapping and retry services.

Public responses contain trace and correlation identifiers, request path and method, error details, validation errors, and retry hints when applicable. Raw query strings, user identifiers, secure details, and internal execution context are never included.

`IOperationContext` is optional. Register it when correlation metadata should be added to responses and logs.

## Logging

The middleware uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics include exception category, error code, trace identifiers, method, path, endpoint, whether a query string was present, and masked secure details. Raw query string values are not logged.

## Migrating From 2.x

- Public JSON responses no longer contain `queryString` or `context`.
- Client-aborted request cancellation is propagated through the ASP.NET Core pipeline.
- Applications no longer need to register `IOperationContext` unless correlation metadata is required.
