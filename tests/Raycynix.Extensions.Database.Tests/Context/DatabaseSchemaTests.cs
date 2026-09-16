using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MsSql.Options;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.PostgreSql.Options;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;

namespace Raycynix.Extensions.Database.Tests.Context;

public sealed class DatabaseSchemaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("application")]
    public void MsSqlDefaultSchema_ShouldApplyWithEntityOverrides(string? schema)
    {
        using var services = BuildMsSqlServices(schema);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        context.Model.GetDefaultSchema().Should().Be(schema);
        context.Model.FindEntityType(typeof(DefaultEntity))!.GetSchema().Should().Be(schema);
        context.Model.FindEntityType(typeof(SchemaEntity))!.GetSchema().Should().Be("sales");
        context.Model.FindEntityType(typeof(RuntimeEntity))!.GetSchema().Should().Be("runtime");
        var table = schema is null ? "[DefaultEntity]" : $"[{schema}].[DefaultEntity]";
        context.Set<DefaultEntity>().ToQueryString().Should().Contain($"FROM {table} AS");
        context.Database.GenerateCreateScript().Should().Contain($"CREATE TABLE {table}");
        context.GetService<IHistoryRepository>().GetCreateScript()
            .Should().Contain("CREATE TABLE [__EFMigrationsHistory]");
    }

    [Fact]
    public void MsSqlDifferentDefaultSchemas_ShouldIsolateModelsInSharedCache()
    {
        using var firstServices = BuildMsSqlServices("first");
        using var secondServices = BuildMsSqlServices("second");
        using var firstScope = firstServices.CreateScope();
        using var secondScope = secondServices.CreateScope();
        using var repeatedScope = firstServices.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var second = secondScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var repeated = repeatedScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        first.GetService<IModelSource>().Should().BeSameAs(second.GetService<IModelSource>());
        first.Model.Should().NotBeSameAs(second.Model);
        repeated.Model.Should().BeSameAs(first.Model);
        first.Set<DefaultEntity>().ToQueryString().Should().Contain("[first].[DefaultEntity]");
        second.Set<DefaultEntity>().ToQueryString().Should().Contain("[second].[DefaultEntity]");
        first.GetService<IModelCacheKeyFactory>().Create(first, true)
            .Should().NotBe(second.GetService<IModelCacheKeyFactory>().Create(second, true));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void MsSqlDefaultSchema_ShouldRejectWhitespace(string schema)
    {
        Action action = () => new MsSqlServerOptions { DefaultSchema = schema }.Validate();
        action.Should().Throw<ArgumentException>();
    }

    private static ServiceProvider BuildMsSqlServices(string? schema)
    {
        var services = new ServiceCollection();
        var builder = services.AddRaycynixDatabase(new ConfigurationBuilder().Build(), options =>
        {
            options.ConnectionString = "Server=localhost;Database=schema_tests;Trusted_Connection=true";
            options.EnableSeed = false;
        }, registerCallerAssembly: false);
        if (schema is null)
            builder.AddMsSql();
        else
            builder.AddMsSql(options => options.DefaultSchema = schema);
        builder.AddAssembly<DefaultEntity>();
        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void PostgreSqlWithoutSchemaConfiguration_ShouldLeaveSchemaResolutionToDatabase()
    {
        var services = new ServiceCollection();
        services.AddRaycynixDatabase(new ConfigurationBuilder().Build(), options =>
            {
                options.ConnectionString = "Host=localhost;Database=schema_tests";
                options.EnableSeed = false;
            }, registerCallerAssembly: false)
            .AddPostgreSql()
            .AddAssembly<DefaultEntity>();
        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        provider.GetRequiredService<PostgreSqlOptions>().DefaultSchema.Should().BeNull();
        context.Model.FindEntityType(typeof(DefaultEntity))!.GetSchema().Should().BeNull();
        context.Set<DefaultEntity>().ToQueryString().Should().Contain("FROM \"DefaultEntity\" AS");
        context.Database.GenerateCreateScript().Should().Contain("CREATE TABLE \"DefaultEntity\" (");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application")]
    public void PostgreSqlDefaultSchema_ShouldApplyWithEntityOverrides(string? schema)
    {
        using var services = BuildServices(schema);
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        context.Model.GetDefaultSchema().Should().Be(schema);
        context.Model.FindEntityType(typeof(DefaultEntity))!.GetSchema().Should().Be(schema);
        context.Model.FindEntityType(typeof(SchemaEntity))!.GetSchema().Should().Be("sales");
        context.Model.FindEntityType(typeof(RuntimeEntity))!.GetSchema().Should().Be("runtime");
        context.GetService<IHistoryRepository>().GetCreateScript()
            .Should().Contain("CREATE TABLE \"__EFMigrationsHistory\"");
    }

    [Fact]
    public void DifferentDefaultSchemas_ShouldNotShareCachedModel()
    {
        using var firstServices = BuildServices("first");
        using var secondServices = BuildServices("second");
        using var firstScope = firstServices.CreateScope();
        using var secondScope = secondServices.CreateScope();
        using var repeatedScope = firstServices.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var second = secondScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var repeated = repeatedScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        // Both contexts use the same EF model cache, so isolation must come from the key.
        first.GetService<IModelSource>().Should().BeSameAs(second.GetService<IModelSource>());
        first.Model.Should().NotBeSameAs(second.Model);
        repeated.Model.Should().BeSameAs(first.Model);
        first.Set<DefaultEntity>().ToQueryString().Should().Contain("first.\"DefaultEntity\"");
        second.Set<DefaultEntity>().ToQueryString().Should().Contain("second.\"DefaultEntity\"");
        first.GetService<IModelCacheKeyFactory>().Create(first, true)
            .Should().NotBe(second.GetService<IModelCacheKeyFactory>().Create(second, true));
    }

    [Fact]
    public void DifferentRuntimeSchemas_ShouldNotShareCachedModel()
    {
        using var firstServices = BuildServices("app", "tenant_a");
        using var secondServices = BuildServices("app", "tenant_b");
        using var firstScope = firstServices.CreateScope();
        using var secondScope = secondServices.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();
        var second = secondScope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

        first.GetService<IModelSource>().Should().BeSameAs(second.GetService<IModelSource>());
        first.Model.Should().NotBeSameAs(second.Model);
        first.Set<RuntimeEntity>().ToQueryString().Should().Contain("tenant_a.\"RuntimeEntity\"");
        second.Set<RuntimeEntity>().ToQueryString().Should().Contain("tenant_b.\"RuntimeEntity\"");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void DefaultSchema_ShouldRejectWhitespace(string schema)
    {
        Action action = () => new PostgreSqlOptions { DefaultSchema = schema }.Validate();
        action.Should().Throw<ArgumentException>();
    }

    private static ServiceProvider BuildServices(string? schema, string runtimeSchema = "runtime")
    {
        var services = new ServiceCollection();
        services.AddSingleton(new RuntimeSchema(runtimeSchema));
        services.AddRaycynixDatabase(new ConfigurationBuilder().Build(), options =>
            {
                options.ConnectionString = "Host=localhost;Database=schema_tests";
                options.EnableSeed = false;
            }, registerCallerAssembly: false)
            .AddPostgreSql(options => options.DefaultSchema = schema)
            .AddAssembly<DefaultEntity>();
        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public void Attribute_ShouldQualifyPostgreSqlQueriesAndCreateScript()
    {
        using var context = new SchemaContext();

        context.Set<SchemaEntity>().ToQueryString().Should().Contain("sales.orders");
        context.Database.GenerateCreateScript().Should().Contain("CREATE TABLE sales.orders");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("application")]
    public void WithoutAttribute_ShouldUseDefaultSchema(string? defaultSchema)
    {
        var model = new ModelBuilder();
        model.HasDefaultSchema(defaultSchema);
        new DefaultConfigurator().Configure(model);

        model.Entity<DefaultEntity>().Metadata.GetSchema().Should().Be(defaultSchema);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FluentSchema_ShouldOverrideAttributeAndPreserveTableNameInEitherOrder(bool schemaFirst)
    {
        var model = new ModelBuilder();
        new SchemaConfigurator().Configure(model);
        var entity = model.Entity<SchemaEntity>();

        if (schemaFirst)
            entity.EntitySchema("archive").EntityName("old_orders");
        else
            entity.EntityName("old_orders").EntitySchema("archive");

        entity.Metadata.GetSchema().Should().Be("archive");
        entity.Metadata.GetTableName().Should().Be("old_orders");
    }

    [Fact]
    public void NullSchema_ShouldResetAttributeToModelDefault()
    {
        var model = new ModelBuilder();
        model.HasDefaultSchema("application");
        new SchemaConfigurator().Configure(model);

        var entity = model.Entity<SchemaEntity>().EntitySchema(null);

        entity.Metadata.GetSchema().Should().Be("application");
        entity.Metadata.GetTableName().Should().Be("orders");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptySchema_ShouldBeRejected(string schema)
    {
        var entity = new ModelBuilder().Entity<SchemaEntity>();

        Action attribute = () => _ = new DatabaseSchemaAttribute(schema);
        Action extension = () => entity.EntitySchema(schema);

        attribute.Should().Throw<ArgumentException>();
        extension.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NullAttributeSchema_ShouldBeRejected()
    {
        Action action = () => _ = new DatabaseSchemaAttribute(null!);
        action.Should().Throw<ArgumentException>();
    }

    private sealed class SchemaContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql("Host=localhost;Database=schema_tests");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("application");
            new SchemaConfigurator().Configure(modelBuilder);
        }
    }

    private sealed class SchemaEntity
    {
        public int Id { get; set; }
    }

    [DatabaseTable("orders")]
    [DatabaseSchema("sales")]
    private sealed class SchemaConfigurator : GenericConfigurator<SchemaEntity>
    {
        public override Type[] DependsOn => [];
    }

    private sealed class DefaultEntity
    {
        public int Id { get; set; }
    }

    private sealed class DefaultConfigurator : GenericConfigurator<DefaultEntity>
    {
        public override Type[] DependsOn => [];
    }

    private sealed record RuntimeSchema(string Name);

    private sealed class RuntimeEntity
    {
        public int Id { get; set; }
    }

    [DatabaseSchema("attribute_schema")]
    private sealed class RuntimeConfigurator(RuntimeSchema? schema = null) : GenericConfigurator<RuntimeEntity>
    {
        public override Type[] DependsOn => [];

        public override void Configure(ModelBuilder modelBuilder) =>
            ConfigureEntity(modelBuilder).EntitySchema(schema?.Name ?? "runtime");

        protected override string? GetModelShapeCacheKey() => schema?.Name ?? "runtime";
    }
}
