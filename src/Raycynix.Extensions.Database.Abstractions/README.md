# Raycynix.Extensions.Database.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

Contracts and configuration models shared by the Raycynix database packages.

## What It Provides

- `DatabaseOptions` and `ConnectionOptions`
- `IDatabaseBuilder`
- `IDatabaseContextServices<TContext>` for context-isolated options and model configuration
- `IDatabaseInitializer`
- `IDatabaseInitializer<TContext>` for initializing one selected context
- `IDatabaseProviderRegistration`
- `IDatabaseProviderModelConfigurator`
- `IDatabaseModelAssemblyRegistry`
- `IDatabaseObservability`
- `IConfigurator` and `IGenericConfigurator<T>`
- `DatabaseTableAttribute`
- `DatabaseSchemaAttribute` for optional schema mapping on `GenericConfigurator<T>` implementations

## Provider Contracts

Provider packages implement `IDatabaseProviderRegistration` to validate provider-specific settings, resolve a connection string, and configure EF Core provider options.

```csharp
public interface IDatabaseProviderRegistration
{
    string ProviderName { get; }

    void Validate(DatabaseOptions configuration);

    string ResolveConnectionString(
        DatabaseOptions configuration,
        IServiceProvider serviceProvider);

    void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseOptions configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider);
}
```

Common validation stays in `DatabaseOptions`. Provider-specific rules, such as whether `Host` or `Username` is required, belong in the provider implementation.

Providers can also register `IDatabaseProviderModelConfigurator` to apply model defaults before
entity configurators. Its `ProviderName` selects the active provider and `ModelCacheKey` must
identify all settings that affect the model. PostgreSQL and SQL Server use this contract for `DefaultSchema`.

## Configurators

Reusable packages can contribute EF Core mappings through configurators:

```csharp
[DatabaseTable("orders")]
public sealed class OrderConfigurator : IGenericConfigurator<Order>
{
    public Type Type => typeof(Order);

    public Type[] DependsOn => [];

    public string ModelCacheKey => typeof(Order).FullName!;

    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();
        entity.ToTable("orders");
        entity.HasKey(static order => order.Id);
    }

    public void Seed(ModelBuilder modelBuilder)
    {
    }
}
```

If a configurator changes the model shape from runtime values, include those values in `ModelCacheKey` so EF Core does not reuse an incompatible cached model.

## Usage

This package is intended for provider packages, optional feature packages, and reusable modules that need database contracts without depending on the core runtime registration package.

## Migrating From 2.x

- `DatabaseConfiguration` is now `DatabaseOptions`.
- `ConnectionConfiguration` is now `ConnectionOptions`.
- Both types are available from `Raycynix.Extensions.Database.Abstractions.Options`.
- Connection properties are now settable from registration callbacks as well as configuration binding.
