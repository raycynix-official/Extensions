# Raycynix.Extensions.Configuration

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Configuration` contains the core typed-configuration registration helpers for Raycynix applications.

## Package

- Target framework: `net10.0`
- Built on `Microsoft.Extensions.Configuration` and `Microsoft.Extensions.Options` 10.x

## What it contains

- `AddRaycynixEnvironment()`
- `AddRaycynixEnvironment(string)`
- `AddRaycynixConfigurationSources(...)`
- `UseRaycynixConfigurationSources(...)`
- `AddEnvFile(...)`
- `AddRaycynixFeatureFlags(...)`
- `AddRaycynixConfiguration<TOptions>(...)`
- `ConfigureRaycynixConfigurationDiagnostics(...)`
- `AddRaycynixConfigurationRedactor<TRedactor>()`
- `AddRaycynixConfigurationRedactor(...)`
- `AddRaycynixConfigurationAccessor<TOptions>()`
- `AddRaycynixConfigurationValidator<TOptions, TValidator>()`
- `AddRaycynixConfigurationValidator<TOptions>(...)`
- `AddRaycynixConfigurationReloadPolicy<TOptions, TReloadPolicy>()`
- `AddRaycynixConfigurationReloadPolicy<TOptions>(...)`
- `AddRaycynixConfigurationChangeHandler<TOptions, THandler>()`
- `AddRaycynixConfigurationChangeHandler<TOptions>(...)`
- typed configuration binding based on the standard Options pipeline
- standard environment abstraction based on `IHostEnvironment`
- standard configuration source ordering
- optional `.env` loading enabled by default
- feature flag access through `IFeatureFlagAccessor`
- named options registration through `optionsName`
- required-section validation through `requireSection`
- support for default values through delegates and `IConfigurationDefaults<TOptions>`
- startup validation through standard `IValidateOptions<TOptions>` integration
- unified typed access through `IConfigurationAccessor<TOptions>`
- reload governance through `IConfigurationReloadPolicy<TOptions>`
- typed change notifications through `IOptionsMonitor<TOptions>`
- diagnostics through `IConfigurationDiagnostics`
- redacted configuration snapshots through `IConfigurationRedactor`
- optional Microsoft.Extensions.Logging diagnostics for configuration reload tracking
- source setup customization through `ConfigurationSourcesOptions`

## What it does not contain

- custom replacement for `IConfiguration`
- custom configuration providers
- feature flag infrastructure
- custom logging provider requirement

## Usage

```csharp
builder.Configuration.UseRaycynixConfigurationSources(options =>
{
    options.BaseFileName = "appsettings";
    options.EnvironmentName = builder.Environment.EnvironmentName;
    options.IncludeUserSecrets = builder.Environment.IsDevelopment();
    options.IncludeEnvFile = true;
});

builder.Services.AddRaycynixEnvironment();
builder.Services.AddRaycynixFeatureFlags(builder.Configuration);

builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    requireSection: true,
    configureDefaults: options =>
    {
        options.TimeoutSeconds = 30;
    });

builder.Services.AddRaycynixConfigurationValidator<MyOptions, MyOptionsValidator>();

builder.Services.AddRaycynixConfigurationReloadPolicy<MyOptions>(context =>
{
    return ConfigurationReloadResult.Reject("MyOptions cannot be changed at runtime.");
});

builder.Services.AddRaycynixConfigurationChangeHandler<MyOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine($"Configuration changed: {context.ChangedAtUtc:O}");
        return ValueTask.CompletedTask;
});

builder.Services.ConfigureRaycynixConfigurationDiagnostics(options =>
{
    options.MaxReloadHistoryPerOptions = 10;
});
```

## Dotenv Files

The standard source setup loads an optional `.env` file from `ConfigurationSourcesOptions.BasePath`.
No additional registration is required:

```dotenv
Database__Host=localhost
Database__Port=5432
API_TOKEN="development token"
```

Double underscores map to configuration sections, so `Database__Host` is available as
`configuration["Database:Host"]`. Blank lines, comments, `export KEY=value`, single-quoted values,
double-quoted values, and inline comments are supported.

Use a different file name, require the file, or disable dotenv loading through source options:

```csharp
builder.Configuration.UseRaycynixConfigurationSources(options =>
{
    options.EnvFileName = ".env.local";
    options.EnvFileOptional = false;
    options.IncludeEnvFile = true;
});
```

For standalone use, add a dotenv file directly:

```csharp
configurationBuilder.AddEnvFile(".env", optional: true, reloadOnChange: false);
```

Dotenv values are added to configuration only; the provider does not mutate process environment
variables. Keep secrets out of source control. The repository ignores `.env` and `.env.*`, while
allowing `.env.example` templates.

## appsettings.json

Typed options bind from a section named after the options type by default:

```json
{
  "MyOptions": {
    "Value": "from-config",
    "TimeoutSeconds": 30
  },
  "FeatureFlags": {
    "Flags": {
      "NewDashboard": true,
      "UseFastCache": false
    }
  }
}
```

Nested paths can use the same convention without repeating type names as string literals:

```csharp
var sectionPath =
    ConfigurationSectionPath.Combine<ParentOptions, ChildOptions>();

builder.Services.AddRaycynixConfiguration<ChildOptions>(
    builder.Configuration,
    sectionName: sectionPath);
```

You can override the section name explicitly when needed:

```csharp
builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    sectionName: "MyFeatureArea:MyOptions");
```

Named options can be registered by passing `optionsName`:

```csharp
builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    sectionName: "Tenants:Primary",
    optionsName: "Primary");
```

`IConfigurationAccessor<TOptions>.Get("Primary")` returns the current approved snapshot for that named options instance.

## Applying Runtime Reload Rules

Use runtime change handling in this order:

1. register the typed options model with `AddRaycynixConfiguration<TOptions>(...)`
2. register a validator if the options must be valid on startup
3. register a reload policy if runtime updates must be limited
4. register one or more change handlers for the updates that are actually allowed

Example:

```csharp
builder.Services.AddRaycynixConfiguration<CacheOptions>(builder.Configuration);

builder.Services.AddRaycynixConfigurationReloadPolicy<CacheOptions>(context =>
{
    if (context.Previous.ConnectionString != context.Current.ConnectionString)
    {
        return ConfigurationReloadResult.Reject(
            "CacheOptions.ConnectionString cannot be changed at runtime.");
    }

    return ConfigurationReloadResult.Apply();
});

builder.Services.AddRaycynixConfigurationChangeHandler<CacheOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine(
            $"Cache options reloaded at {context.ChangedAtUtc:O}. New TTL: {context.Current.DefaultTtlSeconds}");

        return ValueTask.CompletedTask;
    });
```

In this example:

- the options are still reloadable
- connection string changes are rejected
- allowed changes continue to flow through the registered handlers

By default, the package binds the section named after the options type, for example `MyOptions`.

You can override the section name explicitly when needed.

Registered validators run through the standard Options validation pipeline and are enforced on startup through `ValidateOnStart()`.

`AddRaycynixConfiguration<TOptions>(...)` also registers `IConfigurationAccessor<TOptions>` so application services can read the current typed configuration without directly depending on `IOptionsMonitor<TOptions>`.

Example:

```csharp
public class MyService(IConfigurationAccessor<MyOptions> configurationAccessor)
{
    public void Execute()
    {
        var options = configurationAccessor.Current;
        Console.WriteLine(options.TimeoutSeconds);
    }
}
```

Reload policies are evaluated before change handlers are notified. A policy can apply, reject, ignore, or mark a runtime change as requiring restart.

When a runtime change is rejected, `IConfigurationAccessor<TOptions>` continues to expose the last approved configuration snapshot.

Configuration change handlers are triggered through the standard `IOptionsMonitor<TOptions>` pipeline when reloadable sources produce updated option values.

You can also declare simple runtime reload rules directly on properties:

```csharp
public class CacheOptions
{
    [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
    public string ConnectionString { get; set; } = string.Empty;

    public int DefaultTtlSeconds { get; set; }
}
```

When a property marked with `Reject` changes, the runtime update is rejected automatically before handlers are called.

## Diagnostics and Redaction

`IConfigurationDiagnostics` exposes registered options, retained reload decisions, and redacted snapshots:

```csharp
var diagnostics = app.Services.GetRequiredService<IConfigurationDiagnostics>();
var registrations = diagnostics.GetRegistrations();
var reloads = diagnostics.GetReloads();
var snapshot = diagnostics.GetRedactedSnapshot<MyOptions>();
```

Diagnostics behavior can be configured globally:

```csharp
builder.Services.ConfigureRaycynixConfigurationDiagnostics(options =>
{
    options.EnableSnapshots = true;
    options.MaxReloadHistoryPerOptions = 10;
});
```

The default redactor hides common sensitive keys such as passwords, tokens, API keys, private keys, authorization values, and connection strings. Applications can replace it with a custom implementation:

```csharp
builder.Services.AddRaycynixConfigurationRedactor((key, value) =>
{
    return key.Contains("license", StringComparison.OrdinalIgnoreCase)
        ? "***"
        : value;
});
```

## Logging

The package uses the standard `Microsoft.Extensions.Logging.ILogger<T>` abstraction when a logger is available. Logger dependencies are optional, so the package can run without registering a logging provider. It does not require `Raycynix.Extensions.Serilog`; any Microsoft-compatible logging provider can receive the events.

Runtime configuration reload tracking writes operational decisions at `Information` and `Warning`, handler failures at `Error`, and detailed lifecycle diagnostics at `Debug`. Configuration validation and diagnostics snapshot access also emit Debug/Warning events without logging configuration values.

Enable Debug logs for this package when troubleshooting registrations, validation, reload policy decisions, diagnostics snapshots, or change handler execution:

```json
{
  "Logging": {
    "LogLevel": {
      "Raycynix.Extensions.Configuration": "Debug"
    }
  }
}
```

## Feature Flags

Use the standard `FeatureFlags` section to store boolean feature toggles:

```json
{
  "FeatureFlags": {
    "Flags": {
      "NewDashboard": true,
      "UseFastCache": false
    }
  }
}
```

Register the feature flags accessor:

```csharp
builder.Services.AddRaycynixFeatureFlags(builder.Configuration);
```

Use it in application code:

```csharp
public class DashboardService(IFeatureFlagAccessor featureFlags)
{
    public bool UseNewDashboard()
    {
        return featureFlags.IsEnabled("NewDashboard");
    }
}
```

Feature flags use the same configuration pipeline as the rest of the package, so they can also participate in reloadable sources.

The standard source order is:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. user secrets when enabled
4. `.env` when enabled
5. environment variables
6. command-line arguments

The standard environment names are:

- `Development`
- `Testing`
- `Staging`
- `Production`
