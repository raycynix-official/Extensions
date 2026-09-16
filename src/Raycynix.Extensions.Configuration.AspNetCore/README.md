# Raycynix.Extensions.Configuration.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Configuration.AspNetCore` adds ASP.NET Core-specific integrations for Raycynix configuration and feature flags.

## Package

- Target framework: `net10.0`
- Built on ASP.NET Core 10.x

## What it contains

- `AddRaycynixAspNetCoreConfiguration(...)`
- `AddRaycynixFeatureGateOptions(...)`
- `UseRaycynixAspNetCoreConfiguration(...)`
- `[FeatureGate(...)]`
- `RequireFeature(...)`
- `RequireAnyFeature(...)`
- optional Microsoft.Extensions.Logging diagnostics for feature gate middleware
- source setup customization through `ConfigurationSourcesOptions`

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixAspNetCoreConfiguration();
builder.Services.AddRaycynixFeatureGateOptions(options =>
{
    options.DisabledStatusCode = StatusCodes.Status404NotFound;
    options.MissingFeatureFlagAccessorStatusCode = StatusCodes.Status503ServiceUnavailable;
});

builder.Services.AddRaycynixFeatureFlags(builder.Configuration);

var app = builder.Build();

app.UseRaycynixAspNetCoreConfiguration();

app.MapGet("/dashboard", () => Results.Ok("enabled"))
    .RequireFeature("NewDashboard");

app.Run();
```

`AddRaycynixAspNetCoreConfiguration()` loads the optional `.env` file from the application content
root. Values from real environment variables and command-line arguments override dotenv values.
Configure or disable this behavior through `ConfigurationSourcesOptions`:

```csharp
builder.AddRaycynixAspNetCoreConfiguration(options =>
{
    options.EnvFileName = ".env.local";
    options.IncludeEnvFile = true;
});
```

## appsettings.json

```json
{
  "FeatureFlags": {
    "Flags": {
      "NewDashboard": true,
      "BetaApi": false
    }
  }
}
```

Use the attribute for MVC or endpoint metadata scenarios:

```csharp
[FeatureGate("NewDashboard")]
public sealed class DashboardController : ControllerBase
{
}
```

When a required feature flag is disabled, the request returns `404 Not Found` by default. When a gated endpoint is reached without a registered `IFeatureFlagAccessor`, the middleware also returns `404 Not Found` by default. Both status codes can be changed through `AddRaycynixFeatureGateOptions(...)`.

## Logging

The feature gate middleware logs through the standard `Microsoft.Extensions.Logging.ILogger<T>` abstraction when a logger is available. Logger injection is optional, so the middleware can run without registering a logging provider. It works with any Microsoft-compatible logging provider and does not require `Raycynix.Extensions.Serilog`.

The middleware writes detailed gate evaluation flow at `Debug`, missing feature flag accessor diagnostics at `Warning`, and blocked feature-gated endpoints at `Information`.

Enable Debug logs when troubleshooting feature-gated endpoints:

```json
{
  "Logging": {
    "LogLevel": {
      "Raycynix.Extensions.Configuration.AspNetCore": "Debug"
    }
  }
}
```
