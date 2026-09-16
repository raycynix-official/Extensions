# Raycynix.Extensions.Database.AspNetCore.Identity

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

ASP.NET Core Identity integration for `Raycynix.Extensions.Database`.

This package adds Raycynix-compatible `IdentityDbContext` implementations and service registration extensions for applications that store ASP.NET Core Identity data through the Raycynix database infrastructure.

## What It Provides

- Default `RaycynixIdentityDatabaseContext` based on ASP.NET Core Identity.
- Generic identity contexts for custom users, roles, keys, claims, logins, and tokens.
- `AddRaycynixIdentityDatabase` registration methods with strict identity-context constraints.
- Raycynix model assembly discovery and configurator execution.
- Provider validation, database initialization, migrations assembly selection, and model-cache integration inherited from `Raycynix.Extensions.Database`.

This package does not configure authentication, authorization, cookies, token handlers, or `AddIdentity`. It only registers the EF Core database context and Raycynix database infrastructure used by ASP.NET Core Identity.

## Installation Shape

Use this package together with one Raycynix database provider package, for example:

```csharp
using Raycynix.Extensions.Database.AspNetCore.Identity;
using Raycynix.Extensions.Database.PostgreSql;

builder.Services
    .AddRaycynixIdentityDatabase(builder.Configuration)
    .AddPostgreSql();
```

## Default Identity Context

The default context uses ASP.NET Core Identity's standard `IdentityDbContext` shape.

```csharp
builder.Services
    .AddRaycynixIdentityDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnableSeed = false;
    })
    .AddSqlite();
```

## Custom User Type

For a custom string-key user, use the generic Raycynix context:

```csharp
using Microsoft.AspNetCore.Identity;
using Raycynix.Extensions.Database.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public DateTime CreatedAtUtc { get; set; }
}

builder.Services
    .AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext<ApplicationUser>>(
        builder.Configuration)
    .AddPostgreSql();
```

## Custom User, Role, And Key

For a custom key type, use the `TUser`, `TRole`, `TKey` context:

```csharp
using Microsoft.AspNetCore.Identity;
using Raycynix.Extensions.Database.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
}

public sealed class ApplicationRole : IdentityRole<Guid>
{
}

builder.Services
    .AddRaycynixIdentityDatabase<
        RaycynixIdentityDatabaseContext<ApplicationUser, ApplicationRole, Guid>>(
        builder.Configuration)
    .AddPostgreSql();
```

## Fully Customized Identity Entities

The package also includes a fully generic context for custom claim, role mapping, login, role claim, and token entity types:

```csharp
RaycynixIdentityDatabaseContext<
    TUser,
    TRole,
    TKey,
    TUserClaim,
    TUserRole,
    TUserLogin,
    TRoleClaim,
    TUserToken>
```

All Raycynix identity contexts implement `IRaycynixIdentityDatabaseContext`, which keeps `AddRaycynixIdentityDatabase` scoped to Identity contexts instead of arbitrary EF Core `DbContext` types.

## Model And Migrations Assemblies

Use marker types when model configurators and migrations live in known assemblies:

```csharp
builder.Services
    .AddRaycynixIdentityDatabase<
        RaycynixIdentityDatabaseContext<ApplicationUser, ApplicationRole, Guid>,
        ModelAssemblyMarker,
        MigrationsAssemblyMarker>(builder.Configuration)
    .AddMsSql();
```

Use explicit assemblies when markers are not convenient:

```csharp
builder.Services
    .AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext>(
        builder.Configuration,
        migrationsAssembly,
        modelAssembly)
    .AddSqlite();
```

Additional configurator assemblies can be registered through:

```csharp
builder.Services.AddRaycynixDatabaseAssembly<MyModelMarker>();
```

## Database Initialization

Database creation and migration behavior is controlled by `DatabaseOptions`, the same as in the core database package. In ASP.NET Core applications, use the startup helper from `Raycynix.Extensions.Database.AspNetCore` when you want initialization during application startup:

```csharp
await app.InitializeRaycynixDatabaseAsync();
```

## Migrating From 2.x

Identity database registration now uses `DatabaseOptions` from `Raycynix.Extensions.Database.Abstractions.Options`. Rename the root and provider configuration sections to their 3.0 options type names.

## Table Schemas

Identity tables inherit the provider's optional default schema:

```csharp
builder.Services.AddRaycynixIdentityDatabase(builder.Configuration)
    .AddPostgreSql(options => options.DefaultSchema = "identity");
// Or: .AddMsSql(options => options.DefaultSchema = "identity");
```

Omit the option to retain the provider/database default. The common model configurator applies
the schema to Identity mappings and includes it in the model cache key. Explicit per-entity
mappings take precedence. Migration history remains in its standard provider location.
