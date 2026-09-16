# Raycynix.Extensions.Exceptions.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Exceptions.Abstractions` contains the contracts and shared models used by the Raycynix exceptions packages.

## What it contains

- exception contracts such as `IRaycynixException`
- mapping and masking contracts
- retry and background execution contracts
- shared models such as `ErrorExecutionContext`, `ExceptionDetail`, and `RetryExecutionOptions`

## Purpose

This package lets other packages depend on Raycynix exception contracts without taking a dependency on the full implementation package.

Exception contracts validate required messages, machine-readable codes, categories, and HTTP error status codes. Public response contracts intentionally exclude raw query strings and internal execution context.

## Logging

This package contains contracts, options, and shared models only. Runtime diagnostics belong to implementation packages, so this package does not add a logging dependency.

## Example

Libraries can depend only on abstractions when they need retry or masking contracts:

```csharp
public sealed class SyncService(IRetryExecutor retryExecutor)
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return retryExecutor.ExecuteAsync(
            operation: async token =>
            {
                await Task.Delay(10, token);
            },
            operationName: "SyncService.Execute",
            cancellationToken: cancellationToken);
    }
}
```

## Migrating From 2.x

- `IExceptionResponse.QueryString` and `IExceptionResponse.Context` were removed to prevent internal request data from entering public payloads.
- `IExceptionResponse.ValidationErrors` now uses `IReadOnlyDictionary<string, string[]>`.
- `ExceptionDetail` validates required values and is now sealed.
- Custom `RaycynixException` implementations must use status codes between 400 and 599.
