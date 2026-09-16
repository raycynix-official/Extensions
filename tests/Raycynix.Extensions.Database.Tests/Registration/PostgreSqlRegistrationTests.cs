using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.PostgreSql;
using Raycynix.Extensions.Database.PostgreSql.Options;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers PostgreSQL-specific service registration.
/// </summary>
public sealed class PostgreSqlRegistrationTests
{
    /// <summary>
    /// Verifies that PostgreSQL options continue to bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddPostgreSql_ShouldBindOptionsFromDatabaseOptionsSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Host=localhost;Database=test;",
                ["DatabaseOptions:PostgreSqlOptions:DefaultSchema"] = "application",
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
        var options = serviceProvider.GetRequiredService<PostgreSqlOptions>();

        options.Pooling.Should().BeFalse();
        options.DefaultSchema.Should().Be("application");
        options.MinimumPoolSize.Should().Be(2);
        options.MaximumPoolSize.Should().Be(25);
        options.CommandTimeoutSeconds.Should().Be(45);
        options.IncludeErrorDetail.Should().BeTrue();
    }
}
