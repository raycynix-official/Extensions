# Raycynix.Extensions.Configuration.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Configuration.Abstractions` contains the contracts used by the Raycynix configuration packages.

## Package

- Target framework: `net10.0`
- Built on Microsoft.Extensions abstractions 10.x

## What it contains

- `ConfigurationReloadBehaviorAttribute`
- `IApplicationEnvironment`
- `IConfigurationAccessor<TOptions>`
- `IConfigurationChangeHandler<TOptions>`
- `IConfigurationDefaults<TOptions>`
- `IConfigurationDiagnostics`
- `IConfigurationRedactor`
- `IFeatureFlagAccessor`
- `IConfigurationReloadPolicy<TOptions>`
- `IConfigurationValidator<TOptions>`
- `ConfigurationChangeContext<TOptions>`
- `ConfigurationRegistrationInfo`
- `ConfigurationReloadInfo`
- `ConfigurationReloadBehavior`
- `ConfigurationReloadResult`
- `ConfigurationValidationResult`

## Purpose

This package allows applications and libraries to provide their own typed access, feature-flag access, diagnostics, redaction, default-value, validation, reload-governance, and change-handling strategies for typed configuration models without depending on the configuration implementation package.

The abstractions are aligned with the standard Microsoft.Extensions model. Packages that implement runtime diagnostics should use optional `Microsoft.Extensions.Logging.ILogger<T>` dependencies so applications can choose any compatible logging provider or run without one.

## Example

Libraries can depend only on abstractions and consume the current typed configuration through `IConfigurationAccessor<TOptions>`:

```csharp
public sealed class CacheService(IConfigurationAccessor<CacheOptions> configurationAccessor)
{
    public int GetDefaultTtlSeconds()
    {
        return configurationAccessor.Current.DefaultTtlSeconds;
    }
}
```

Runtime reload rules can also be expressed at the options model level:

```csharp
public sealed class CacheOptions
{
    [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
    public string ConnectionString { get; init; } = string.Empty;

    public int DefaultTtlSeconds { get; init; }
}
```

Diagnostics consumers can depend on `IConfigurationDiagnostics` to inspect registered options, retained reload decisions, and redacted snapshots without referencing the implementation package directly.
