# Raycynix.Extensions.Security

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Security` contains the core, host-agnostic security implementation for Raycynix applications.

## What it contains

- `SecurityContext`
- `AddRaycynixSecurity(...)`
- `SecurityOptions`
- `JwtOptions`
- DI registration for `ISecurityContext`

## What it does not contain

- ASP.NET Core authentication handlers
- HTTP request mapping
- JWT validation middleware
- authorization policies
- secret storage and resolution

## Usage

```csharp
builder.Services.AddRaycynixSecurity(builder.Configuration, options =>
{
    options.JwtOptions.Authority = "https://auth.raycynix.com";
    options.JwtOptions.Issuer = "raycynix-auth";
    options.JwtOptions.Audience = "raycynix-services";
});
```

```json
{
  "SecurityOptions": {
    "JwtOptions": {
      "Authority": "https://auth.raycynix.com",
      "Issuer": "raycynix-auth",
      "Audience": "raycynix-services",
      "AccessTokenLifetime": "00:15:00",
      "RefreshTokenLifetime": "14.00:00:00",
      "ClockSkew": "00:01:00",
      "RequireHttpsMetadata": true
    }
  }
}
```

The package binds settings from the `SecurityOptions` section and allows optional overrides in code.

Both the root and nested options are available through dependency injection and use the same bound snapshot:

```csharp
public sealed class TokenService(SecurityOptions security, JwtOptions jwt)
{
    public bool UsesSharedSnapshot => ReferenceEquals(security.JwtOptions, jwt);
}
```

For ASP.NET Core request binding and web-specific integration, use `Raycynix.Extensions.Security.AspNetCore`.

For secret resolution, use `Raycynix.Extensions.Secrets`.

## Logging

This package contains host-agnostic registration, configuration, and the default security context model. Runtime
authentication and authorization diagnostics belong to host integration packages, so this package does not add a logging
dependency.

## Migrating From 2.x

- Replace `SecurityConfiguration` with `SecurityOptions`.
- Replace `JwtConfiguration` with `JwtOptions`.
- Replace the `Raycynix.Extensions.Security.Configurations` namespace with `Raycynix.Extensions.Security.Options`.
- Rename the root configuration section from `SecurityConfiguration` to `SecurityOptions` and the nested key from `Jwt` to `JwtOptions`.
- Remove separate `JwtOptions` binding or construction. `AddRaycynixSecurity(...)` now registers the bound nested options directly.
