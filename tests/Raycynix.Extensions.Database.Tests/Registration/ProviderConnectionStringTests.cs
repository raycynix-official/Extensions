using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Npgsql;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MySql;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.Sqlite;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers provider-specific connection-string resolution behavior.
/// </summary>
public sealed class ProviderConnectionStringTests
{
    /// <summary>
    /// Verifies that PostgreSQL connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void PostgreSqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:PostgreSqlOptions:Pooling"] = "false",
                ["DatabaseOptions:PostgreSqlOptions:MinimumPoolSize"] = "2",
                ["DatabaseOptions:PostgreSqlOptions:MaximumPoolSize"] = "25",
                ["DatabaseOptions:PostgreSqlOptions:CommandTimeoutSeconds"] = "45",
                ["DatabaseOptions:PostgreSqlOptions:IncludeErrorDetail"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddPostgreSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "postgresql");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionOptions("db.local", 5433, "orders", "app", "secret"),
            serviceProvider);

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        builder.Host.Should().Be("db.local");
        builder.Port.Should().Be(5433);
        builder.Database.Should().Be("orders");
        builder.Username.Should().Be("app");
        builder.Password.Should().Be("secret");
        builder.Pooling.Should().BeFalse();
        builder.MinPoolSize.Should().Be(2);
        builder.MaxPoolSize.Should().Be(25);
        builder.CommandTimeout.Should().Be(45);
        builder.IncludeErrorDetail.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that SQL Server connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void MsSqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:MsSqlServerOptions:TrustServerCertificate"] = "false",
                ["DatabaseOptions:MsSqlServerOptions:MultipleActiveResultSets"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddMsSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlserver");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionOptions("sql.local", null, "orders", "sa", "secret"),
            serviceProvider);

        var builder = new SqlConnectionStringBuilder(connectionString);
        builder.DataSource.Should().Be("sql.local");
        builder.InitialCatalog.Should().Be("orders");
        builder.UserID.Should().Be("sa");
        builder.Password.Should().Be("secret");
        builder.TrustServerCertificate.Should().BeFalse();
        builder.MultipleActiveResultSets.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MySQL connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void MySqlRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:MySqlOptions:AllowUserVariables"] = "false",
                ["DatabaseOptions:MySqlOptions:Pooling"] = "false"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddMySql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "mysql");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionOptions("mysql.local", 3307, "orders", "app", "secret"),
            serviceProvider);

        var builder = new MySqlConnectionStringBuilder(connectionString);
        builder.Server.Should().Be("mysql.local");
        builder.Port.Should().Be(3307);
        builder.Database.Should().Be("orders");
        builder.UserID.Should().Be("app");
        builder.Password.Should().Be("secret");
        builder.AllowUserVariables.Should().BeFalse();
        builder.Pooling.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that SQLite connection-string resolution applies provider-specific settings.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldBuildConnectionStringFromStructuredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:SqliteOptions:Mode"] = "ReadWriteCreate",
                ["DatabaseOptions:SqliteOptions:Cache"] = "Shared"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlite");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionOptions(null, null, "orders.db", null, null),
            serviceProvider);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        builder.DataSource.Should().Be("orders.db");
        builder.Mode.Should().Be(SqliteOpenMode.ReadWriteCreate);
        builder.Cache.Should().Be(SqliteCacheMode.Shared);
    }

    /// <summary>
    /// Verifies that empty SQLite mode and cache values are treated as unset.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldIgnoreEmptyModeAndCacheValues()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:SqliteOptions:Mode"] = "",
                ["DatabaseOptions:SqliteOptions:Cache"] = "   "
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var registration = GetProviderRegistration(serviceProvider, "sqlite");

        var connectionString = registration.ResolveConnectionString(
            CreateConnectionOptions(null, null, "orders.db", null, null),
            serviceProvider);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        builder.DataSource.Should().Be("orders.db");
        builder.Mode.Should().Be(SqliteOpenMode.ReadWriteCreate);
        builder.Cache.Should().Be(SqliteCacheMode.Default);
    }

    /// <summary>
    /// Verifies that invalid SQLite mode values fail through options validation.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldFail_WhenSqliteModeIsInvalid()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:SqliteOptions:Mode"] = "NotARealMode"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddSqlite();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var act = () => GetProviderRegistration(serviceProvider, "sqlite");

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*Mode contains unsupported value 'NotARealMode'*");
    }

    /// <summary>
    /// Verifies that SQL Server structured configuration validates provider-required fields.
    /// </summary>
    [Fact]
    public void MsSqlRegistration_ShouldValidateStructuredConnectionOptions()
    {
        var registration = CreateProviderRegistration(static builder => builder.AddMsSql(), "sqlserver");

        var act = () => registration.Validate(CreateConnectionOptions(null, null, "orders", "sa", "secret"));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*SQL Server connection requires a host*");
    }

    /// <summary>
    /// Verifies that MySQL structured configuration validates provider-required fields.
    /// </summary>
    [Fact]
    public void MySqlRegistration_ShouldValidateStructuredConnectionOptions()
    {
        var registration = CreateProviderRegistration(static builder => builder.AddMySql(), "mysql");

        var act = () => registration.Validate(CreateConnectionOptions("mysql.local", null, "orders", null, "secret"));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*MySQL connection requires a username*");
    }

    /// <summary>
    /// Verifies that PostgreSQL structured configuration validates provider-required fields.
    /// </summary>
    [Fact]
    public void PostgreSqlRegistration_ShouldValidateStructuredConnectionOptions()
    {
        var registration = CreateProviderRegistration(static builder => builder.AddPostgreSql(), "postgresql");

        var act = () => registration.Validate(CreateConnectionOptions("pg.local", null, "orders", null, "secret"));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*PostgreSQL connection requires a username*");
    }

    /// <summary>
    /// Verifies that SQLite structured configuration validates provider-required fields.
    /// </summary>
    [Fact]
    public void SqliteRegistration_ShouldValidateStructuredConnectionOptions()
    {
        var registration = CreateProviderRegistration(static builder => builder.AddSqlite(), "sqlite");

        var act = () => registration.Validate(CreateConnectionOptions(null, null, null, null, null));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*SQLite connection requires a data source name*");
    }

    /// <summary>
    /// Verifies that provider validation is skipped when a raw connection string is provided.
    /// </summary>
    [Fact]
    public void ProviderRegistration_ShouldSkipStructuredValidation_WhenConnectionStringIsProvided()
    {
        var registration = CreateProviderRegistration(static builder => builder.AddPostgreSql(), "postgresql");
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Host=pg.local;Database=orders;",
            ConnectionOptions = new TestConnectionOptions()
        };

        var act = () => registration.Validate(configuration);

        act.Should().NotThrow();
    }

    private static IDatabaseProviderRegistration GetProviderRegistration(IServiceProvider serviceProvider,
        string providerName)
    {
        return serviceProvider
            .GetRequiredService<IEnumerable<IDatabaseProviderRegistration>>()
            .Single(registration => registration.ProviderName == providerName);
    }

    private static IDatabaseProviderRegistration CreateProviderRegistration(
        Func<IDatabaseBuilder, IDatabaseBuilder> registerProvider,
        string providerName)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var builder = services.AddRaycynixDatabase(configuration, registerCallerAssembly: false);
        registerProvider(builder);

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        return GetProviderRegistration(serviceProvider, providerName);
    }

    private static DatabaseOptions CreateConnectionOptions(
        string? host,
        int? port,
        string? name,
        string? username,
        string? password)
    {
        return new DatabaseOptions
        {
            ConnectionOptions = new TestConnectionOptions
            {
                Host = host,
                Port = port,
                Name = name,
                Username = username,
                Password = password
            }
        };
    }

    private sealed class TestConnectionOptions : ConnectionOptions;
}