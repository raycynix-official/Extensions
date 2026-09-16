# Raycynix.Extensions.Security.AspNetCore

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Security.AspNetCore` adds ASP.NET Core JWT authentication, dynamic authorization policies, and shared authorization-attribute integration for Raycynix security.

## What it contains

- `AddRaycynixAspNetCoreSecurity(...)`
- `UseRaycynixSecurity(this IApplicationBuilder app)`
- `AddRaycynixRateLimiting(...)`
- `UseRaycynixRateLimiting(this IApplicationBuilder app)`
- per-request `ClaimsPrincipal` to `ISecurityContext` mapping
- dynamic API policies for `authenticated`, `permission:*`, `role:*`, and `subject:*`
- MVC convention support for shared security attributes from `Raycynix.Extensions.Security.Abstractions`
- endpoint-builder helpers through `RequireRaycynixAuthorization(...)`
- consistent `401 Unauthorized` and `403 Forbidden` JSON responses

## Usage

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreSecurity(builder.Configuration, options =>
{
    options.JwtOptions.Authority = "https://auth.raycynix.com";
    options.JwtOptions.Issuer = "raycynix-auth";
    options.JwtOptions.Audience = "raycynix-services";
});
builder.Services.AddRaycynixRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseRaycynixRateLimiting();
app.UseAuthorization();

app.Run();
```

```json
{
  "SecurityOptions": {
    "JwtOptions": {
      "Authority": "https://auth.raycynix.com",
      "Issuer": "raycynix-auth",
      "Audience": "raycynix-services",
      "RequireHttpsMetadata": true
    }
  }
}
```

## Rate Limiting

Rate limiting is opt-in and uses the built-in ASP.NET Core rate limiting middleware. A global policy can protect every request, while named policies can be selected with the standard `RequireRateLimiting(...)` endpoint extension.

When `Subject` partitioning is used, place rate limiting after authentication and before authorization so the validated JWT subject is available and rejected protected requests are still limited.

```json
{
  "RateLimitOptions": {
    "GlobalPolicy": {
      "Algorithm": "FixedWindow",
      "PartitionStrategy": "IpAddress",
      "PermitLimit": 100,
      "Window": "00:01:00",
      "QueueLimit": 0
    },
    "Policies": {
      "authentication": {
        "Algorithm": "TokenBucket",
        "PartitionStrategy": "Subject",
        "PermitLimit": 10,
        "Window": "00:01:00",
        "TokensPerPeriod": 2
      }
    },
    "RejectionStatusCode": 429,
    "IncludeRetryAfterHeader": true
  }
}
```

```csharp
builder.Services.AddRaycynixRateLimiting(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseRaycynixRateLimiting();
app.UseAuthorization();

app.MapPost("/auth/login", HandleLoginAsync)
    .RequireRateLimiting("authentication");
```

Supported algorithms are `FixedWindow`, `SlidingWindow`, `TokenBucket`, and `Concurrency`. Requests can be partitioned by `IpAddress`, authenticated JWT `Subject` with IP fallback, or one shared `Global` bucket. Rejected requests receive a stable JSON response with status `429` by default.

Use authorization policies with standard names:

```csharp
using Raycynix.Extensions.Security.AspNetCore.Authorization;

[Authorize(Policy = SecurityPolicies.Permission("users.read"))]
[Authorize(Policy = SecurityPolicies.AnyPermission("users.read", "users.write"))]
[Authorize(Policy = SecurityPolicies.AllPermissions("users.read", "users.export"))]
[Authorize(Policy = SecurityPolicies.Role("admin"))]
[Authorize(Policy = SecurityPolicies.AnyRole("admin", "support"))]
[Authorize(Policy = SecurityPolicies.AllRoles("manager", "auditor"))]
[Authorize(Policy = SecurityPolicies.Authenticated)]
[Authorize(Policy = SecurityPolicies.ServiceOnly)]
```

Or use the shared security attributes and let the package translate them into standard ASP.NET Core authorization policies:

```csharp
using Raycynix.Extensions.Security.Abstractions.Attributes;

[RequireAuthenticatedSubject]
[RequireSubjectType(SecuritySubjectType.Service)]
[RequirePermission("users.read")]
public sealed class UsersController : ControllerBase
{
}
```

For minimal APIs or endpoint builders, use the helper extension:

```csharp
app.MapGet("/users/{id}", HandleUserAsync)
    .RequireRaycynixAuthorization(
        new RequireAuthenticatedSubjectAttribute(),
        new RequirePermissionAttribute("users.read"));
```

The package expects JWT access tokens with:

- `sub`
- `subject_type`
- `roles`
- `permissions`

`subject_type` is mapped to `SecuritySubjectType`, allowing both `User` and `Service` request subjects to use the same `ISecurityContext`.

Authentication and authorization failures return safe JSON responses without exposing internal policy details.

JWT bearer authentication, `SecurityOptions`, and directly injected `JwtOptions` share one configuration snapshot. The package no longer performs a separate manual bind.

## Logging

The ASP.NET Core package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover JWT challenges, request security context mapping, dynamic policy resolution, authorization requirement outcomes, generated 401/403 responses, and rate limit rejections. Access tokens, subject identifiers, claim values, role names, permission names, partition keys, and raw policy names are not logged.

## Migrating From 2.x

- Use `SecurityOptions` and `JwtOptions` from `Raycynix.Extensions.Security.Options`.
- Rename the configuration root from `SecurityConfiguration` to `SecurityOptions`.
- Missing or invalid authority is now reported through options validation when the security options are resolved.
- Authorization attributes can now be applied directly to controller actions.
