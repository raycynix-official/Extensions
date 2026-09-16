# Raycynix.Extensions.Security.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Security.Abstractions` contains the transport-neutral contracts used by the Raycynix security and secrets packages.

## What it contains

- `ISecurityContext`
- `ISecretProvider`
- `ISecretResolver`
- `SecuritySubjectType`
- `SecurityClaimTypes`
- shared authorization attributes such as `RequirePermissionAttribute` and `RequireRoleAttribute`

## Purpose

This package allows other packages to depend on shared security, authorization, and secret-resolution contracts without depending on implementation packages.

The security model assumes:

- request subjects are authenticated and known
- supported subject types are `User` and `Service`
- `Roles` are aggregates
- `Permissions` are the canonical access checks

The authorization model exposes declarative attributes that can be reused by multiple pipelines, including messaging and ASP.NET Core:

- `RequireAuthenticatedSubjectAttribute`
- `RequireSubjectTypeAttribute`
- `RequirePermissionAttribute`
- `RequireAnyPermissionAttribute`
- `RequireAllPermissionsAttribute`
- `RequireRoleAttribute`
- `RequireAnyRoleAttribute`
- `RequireAllRolesAttribute`

Authorization attributes can be applied to classes or methods. Permission and role values are trimmed and validated when an attribute is created; empty requirement sets are rejected.

The secret model assumes:

- applications read secrets through shared abstractions
- a provider represents a single source
- a resolver aggregates providers and returns the first available secret
- built-in providers can target local env, GitHub Actions, and TeamCity-style injection

Permissions should use a stable `resource.action` format, for example `users.read` or `orders.approve`.

## Logging

This package contains contracts, attributes, constants, records, and enums only. Runtime diagnostics belong to implementation packages, so this package does not add a logging dependency.

## Usage

```csharp
public sealed class UserProjection(ISecurityContext securityContext)
{
    public string? CurrentSubjectId => securityContext.IsAuthenticated
        ? securityContext.SubjectId
        : null;
}
```
