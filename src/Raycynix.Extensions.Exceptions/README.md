# Raycynix.Extensions.Exceptions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Exceptions` is the core exception handling package.

## What it contains

- `AddRaycynixExceptions(...)`
- exception mapping
- secure data masking
- transient exception classification
- retry execution services
- background task execution helpers
- optional Microsoft `ILogger<T>` diagnostics for retry and background execution
- common Raycynix exception types

## What it does not contain

- ASP.NET Core middleware
- `IApplicationBuilder` extensions
- HTTP response formatting

## Usage

```csharp
builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<InvalidOperationException>(
        errorCode: "invalid_operation",
        message: "The operation is not valid.",
        statusCode: 400);
});
```

You can also customize mappings for domain-specific exceptions:

```csharp
builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<UnauthorizedAccessException>(
        errorCode: "access_denied",
        message: "You do not have permission to perform this action.",
        statusCode: 403);
});
```

For ASP.NET Core request pipeline integration, add `Raycynix.Extensions.Exceptions.AspNetCore`.

## Logging

The package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover retry attempts, retry exhaustion, background operation cancellation, and background operation failures. The package avoids logging secure detail payloads directly.

## Contract Guarantees

- custom mappings must provide a non-empty error code and public message
- mapped HTTP status codes must be between 400 and 599
- configured mappings and validation errors are snapshotted during construction
- retry execution restores the previous error execution context after completion
- user-provided mapper, masker, classifier, retry executor, and background runner registrations are preserved

## Migrating From 2.x

- `ExceptionMapperOptions.Mappings` is now read-only.
- Invalid mapping delegates, status codes, error codes, and messages fail during setup.
- `ValidationException.ValidationErrors` now exposes `IReadOnlyDictionary<string, string[]>`.
- Null operation delegates and blank background operation names are rejected.
