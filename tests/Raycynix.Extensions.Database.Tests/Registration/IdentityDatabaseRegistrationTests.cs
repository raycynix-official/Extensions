using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Attributes;
using Raycynix.Extensions.Database.AspNetCore.Identity;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Infrastructure;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.MsSql;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers service registration for the ASP.NET Core Identity database package.
/// </summary>
public sealed class IdentityDatabaseRegistrationTests
{
    [Theory]
    [InlineData(false, null)]
    [InlineData(false, "identity")]
    [InlineData(true, null)]
    [InlineData(true, "identity")]
    public void IdentityTables_ShouldInheritProviderDefaultSchema(bool sqlServer, string? schema)
    {
        var services = CreateServices();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseOptions:ConnectionString"] = sqlServer
                ? "Server=localhost;Database=identity_tests;Trusted_Connection=true"
                : "Host=localhost;Database=identity_tests",
            ["DatabaseOptions:EnableSeed"] = "false"
        }).Build();
        var builder = services.AddRaycynixIdentityDatabase(configuration, registerCallerAssembly: false);
        if (sqlServer)
            builder.AddMsSql(options => options.DefaultSchema = schema);
        else
            builder.AddPostgreSql(options => options.DefaultSchema = schema);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixIdentityDatabaseContext>();

        context.Model.GetEntityTypes().Should().HaveCount(7);
        context.Model.GetEntityTypes().Should().OnlyContain(entity => entity.GetSchema() == schema);
        var table = sqlServer ? "[AspNetUsers]" : "\"AspNetUsers\"";
        var qualifiedTable = schema is null ? table : sqlServer ? $"[{schema}].{table}" : $"{schema}.{table}";
        context.Users.ToQueryString().Should().Contain($"FROM {qualifiedTable} AS");
        context.Database.GenerateCreateScript().Should().Contain($"CREATE TABLE {qualifiedTable}");
    }

    /// <summary>
    /// Verifies that the default Identity database registration resolves the default context and Identity model.
    /// </summary>
    [Fact]
    public void AddRaycynixIdentityDatabase_ShouldRegisterDefaultIdentityContext()
    {
        var services = CreateServices();

        services.AddRaycynixIdentityDatabase(BuildConfiguration("identity-default.db"), registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<RaycynixIdentityDatabaseContext>();
        var databaseContext = scope.ServiceProvider.GetRequiredService<IRaycynixDatabaseContext>();

        initializer.Should().BeOfType<DatabaseInitializer<RaycynixIdentityDatabaseContext>>();
        databaseContext.Should().BeSameAs(context);
        context.Model.FindEntityType(typeof(IdentityUser)).Should().NotBeNull();
        context.Model.FindEntityType(typeof(IdentityRole)).Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the Identity registration supports a custom user entity.
    /// </summary>
    [Fact]
    public void AddRaycynixIdentityDatabase_ShouldRegisterCustomUserContext()
    {
        var services = CreateServices();

        services
            .AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext<TestIdentityUser>>(
                BuildConfiguration("identity-custom-user.db"),
                registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<RaycynixIdentityDatabaseContext<TestIdentityUser>>();

        context.Should().BeAssignableTo<IRaycynixIdentityDatabaseContext>();
        context.Model.FindEntityType(typeof(TestIdentityUser)).Should().NotBeNull();
        context.Model.FindEntityType(typeof(IdentityRole)).Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the Identity registration supports custom user, role, and key types.
    /// </summary>
    [Fact]
    public void AddRaycynixIdentityDatabase_ShouldRegisterCustomRoleAndKeyContext()
    {
        var services = CreateServices();

        services
            .AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext<GuidIdentityUser, GuidIdentityRole, Guid>>(
                BuildConfiguration("identity-guid-key.db"),
                registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<RaycynixIdentityDatabaseContext<GuidIdentityUser, GuidIdentityRole, Guid>>();

        context.Model.FindEntityType(typeof(GuidIdentityUser)).Should().NotBeNull();
        context.Model.FindEntityType(typeof(GuidIdentityRole)).Should().NotBeNull();
        context.Model.FindEntityType(typeof(IdentityUserClaim<Guid>)).Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that additional Raycynix model configurators are applied to an Identity context.
    /// </summary>
    [Fact]
    public void AddRaycynixIdentityDatabase_ShouldApplyAdditionalModelConfigurators()
    {
        var services = CreateServices();

        services
            .AddRaycynixIdentityDatabase(BuildConfiguration("identity-configurators.db"), registerCallerAssembly: false)
            .AddSqlite()
            .AddAssembly<IdentityProfileEntity>();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<RaycynixIdentityDatabaseContext>();
        var entityType = context.Model.FindEntityType(typeof(IdentityProfileEntity));

        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("identity_profiles");
    }

    /// <summary>
    /// Verifies that a different context type cannot be registered after the initial database registration.
    /// </summary>
    [Fact]
    public void AddRaycynixIdentityDatabase_ShouldFail_WhenDifferentContextIsRegisteredAgain()
    {
        var services = CreateServices();
        var configuration = BuildConfiguration("identity-repeat.db");

        services.AddRaycynixIdentityDatabase(configuration, registerCallerAssembly: false);

        var act = () => services.AddRaycynixIdentityDatabase<RaycynixIdentityDatabaseContext<TestIdentityUser>>(
            configuration,
            registerCallerAssembly: false);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*already registered with context type*RaycynixIdentityDatabaseContext*");
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
        return services;
    }

    private static IConfiguration BuildConfiguration(string databaseName)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = $"Data Source={databaseName}",
                ["DatabaseOptions:EnsureCreated"] = "false",
                ["DatabaseOptions:EnableSeed"] = "false"
            })
            .Build();
    }

    [DatabaseTable("identity_profiles")]
    private sealed class IdentityProfileEntityConfigurator : GenericConfigurator<IdentityProfileEntity>
    {
        public override Type[] DependsOn => [];
    }

    private sealed class IdentityProfileEntity
    {
        public int Id { get; init; }
    }

    private sealed class TestIdentityUser : IdentityUser;

    private sealed class GuidIdentityUser : IdentityUser<Guid>;

    private sealed class GuidIdentityRole : IdentityRole<Guid>;

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
