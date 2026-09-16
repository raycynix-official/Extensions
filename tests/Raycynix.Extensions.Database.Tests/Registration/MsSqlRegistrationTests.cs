using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.MsSql;
using Raycynix.Extensions.Database.MsSql.Options;

namespace Raycynix.Extensions.Database.Tests.Registration;

/// <summary>
/// Covers SQL Server-specific service registration.
/// </summary>
public sealed class MsSqlRegistrationTests
{
    /// <summary>
    /// Verifies that SQL Server options bind from the nested database configuration section.
    /// </summary>
    [Fact]
    public void AddMsSql_ShouldBindOptionsFromDatabaseOptionsSection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseOptions:ConnectionString"] = "Server=localhost;Database=test;",
                ["DatabaseOptions:MsSqlServerOptions:DefaultSchema"] = "application",
                ["DatabaseOptions:MsSqlServerOptions:TrustServerCertificate"] = "false",
                ["DatabaseOptions:MsSqlServerOptions:CommandTimeoutSeconds"] = "45",
                ["DatabaseOptions:MsSqlServerOptions:MultipleActiveResultSets"] = "true"
            })
            .Build();

        services.AddRaycynixDatabase(configuration, registerCallerAssembly: false)
            .AddMsSql();

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        var options = serviceProvider.GetRequiredService<MsSqlServerOptions>();

        options.TrustServerCertificate.Should().BeFalse();
        options.DefaultSchema.Should().Be("application");
        options.CommandTimeoutSeconds.Should().Be(45);
        options.MultipleActiveResultSets.Should().BeTrue();
    }
}
