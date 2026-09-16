# Raycynix.Extensions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions` is a set of infrastructure packages for .NET applications. The repository is organized as a modular collection of packages that can be combined selectively instead of pulling a single monolithic framework into every service.

## Versioning and release notes

The current shared package version is **3.0.1**, declared once in the repository-root
[Directory.Build.props](Directory.Build.props). All published packages under `src/`
inherit this version; do not add package-local `Version` or `PackageVersion` overrides.
Use matching release versions when combining Raycynix packages.

For a release, update `Version` in `Directory.Build.props`, the [release overview](CHANGELOG.md),
and each package's changelog. NuGet release notes inherit `$(Version)` and link to those changelogs.
Test and example projects are not published packages and may retain their own version metadata.

Version 3.0.1 adds optional database schema attributes and fluent mapping, PostgreSQL and SQL Server
default-schema options, and schema-aware EF Core model caching. Schema configuration remains
optional. See the [PostgreSQL schema guide](src/Raycynix.Extensions.Database.PostgreSql/README.md#table-schemas).

## Package groups

The solution is split by responsibility. The main package groups are:

- configuration and configuration abstractions
- contracts and contract integrations for ASP.NET Core
- database core, provider packages, hosting integration, and ASP.NET Core integration
- exceptions and exception handling integration
- structured logging with optional Elasticsearch integration
- messaging core plus transport-specific packages
- metrics, tracing, and observability
- security and secrets

Each package has its own `README.md` under `src/<PackageName>/README.md` with package-specific setup and usage examples.

## Design principles

The packages in this repository follow a few consistent rules:

- composition over inheritance: infrastructure is added through DI registration and small extension packages
- explicit package boundaries: abstractions, core runtime, provider packages, and hosting adapters are separated
- provider registration over magic configuration: relational database providers are selected by adding the matching package and calling its registration method
- shared model composition: reusable packages can contribute EF Core configurators into a shared `DatabaseContext`
- provider-agnostic runtime behavior where possible: messaging persistence and database composition avoid hard-coding provider-specific behavior into the higher-level infrastructure layer

## Database overview

The database module is split into several packages:

- `Raycynix.Extensions.Database`
  Contains the shared `DatabaseContext`, database configuration, configurator discovery, model caching, and `IDatabaseInitializer`.
- `Raycynix.Extensions.Database.Sqlite`
- `Raycynix.Extensions.Database.PostgreSql`
- `Raycynix.Extensions.Database.MsSql`
- `Raycynix.Extensions.Database.MySql`
  Each provider package wires EF Core to the matching relational database.
- `Raycynix.Extensions.Database.Hosting`
  Adds generic-host helpers for database initialization.
- `Raycynix.Extensions.Database.AspNetCore`
  Adds ASP.NET Core helpers for database initialization.

`AddRaycynixDatabase(...)` registers the shared infrastructure only. It does not select a provider by itself, and it does not initialize the database automatically.

## Database setup

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
        options.EnsureCreated = false;
    })
    .AddPostgreSql();
```

If a package contributes EF Core configurators to the shared model, register its assembly explicitly:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration)
    .AddPostgreSql()
    .AddAssembly<SomeModelMarker>();
```

When assembly discovery must be controlled explicitly, disable automatic caller assembly registration:

```csharp
builder.Services
    .AddRaycynixDatabase(builder.Configuration, registerCallerAssembly: false)
    .AddPostgreSql()
    .AddAssembly<IdentityModelMarker>()
    .AddAssembly<AuditModelMarker>();
```

This is useful in tests, plugin-style systems, or solutions where a single assembly may contain configurators with optional dependencies.

## Database initialization

Initialization is performed by `IDatabaseInitializer`, which creates a scoped `DatabaseContext` and then:

1. runs `EnsureCreatedAsync` when `EnsureCreated` is enabled
2. runs `MigrateAsync` when `UseMigrations` is enabled

The hosting packages decide when this initializer should be executed.

### ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql();

var app = builder.Build();

await app.UseRaycynixDatabaseInitializationAsync();

app.Run();
```

### Generic host

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddRaycynixDatabase(builder.Configuration, options =>
    {
        options.UseMigrations = true;
    })
    .AddPostgreSql();

var host = builder.Build();

await host.InitializeRaycynixDatabaseAsync();
await host.RunAsync();
```

## Messaging overview

`Raycynix.Extensions.Messaging` contains the transport-agnostic messaging core. Transport packages such as Kafka, RabbitMQ, HTTP JSON, and gRPC attach concrete delivery mechanisms.

`Raycynix.Extensions.Messaging.Database` adds persistent inbox and outbox storage on top of the shared database infrastructure. It uses EF Core configurators registered into the shared `DatabaseContext` and relies on optimistic concurrency for inbox and outbox lease transitions rather than provider-specific SQL behavior in the messaging layer.

## Logging overview

The 3.0 logging packages target .NET 10 and use the standard
`Microsoft.Extensions.Logging.ILogger<T>` API. `Raycynix.Extensions.Serilog`
provides Serilog host registration, native configuration, service metadata,
ordered extension points, and fallback console output. Optional packages extend
the same pipeline:

- `Raycynix.Extensions.Serilog.Elastic` sends events to Elasticsearch.
- `Raycynix.Extensions.Serilog.Aspire` configures logging for an Aspire AppHost process.

## Package-level documentation

Start with these package READMEs for details:

- `src/Raycynix.Extensions.Database/README.md`
- `src/Raycynix.Extensions.Database.Hosting/README.md`
- `src/Raycynix.Extensions.Database.AspNetCore/README.md`
- `src/Raycynix.Extensions.Messaging/README.md`
- `src/Raycynix.Extensions.Messaging.Database/README.md`
- `src/Raycynix.Extensions.Serilog/README.md`
- `src/Raycynix.Extensions.Serilog.Elastic/README.md`
- `src/Raycynix.Extensions.Serilog.Aspire/README.md`

The remaining packages follow the same pattern: core package, optional abstractions package, and optional hosting or transport adapters where needed.
