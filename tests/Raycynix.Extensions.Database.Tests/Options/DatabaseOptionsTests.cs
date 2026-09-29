using FluentAssertions;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.Tests.Options;

/// <summary>
/// Covers validation and defaults for <see cref="DatabaseOptions"/>.
/// </summary>
public sealed class DatabaseOptionsTests
{
    /// <summary>
    /// Verifies that defaults match the package contract.
    /// </summary>
    [Fact]
    public void Defaults_ShouldMatchExpectedValues()
    {
        var configuration = new DatabaseOptions();

        configuration.EnsureCreated.Should().BeTrue();
        configuration.EnableSeed.Should().BeTrue();
        configuration.EnableAutoDetectChanges.Should().BeTrue();
        configuration.UseQueryTrackingByDefault.Should().BeTrue();
        configuration.RetryCount.Should().Be(5);
        configuration.RetryDelaySeconds.Should().Be(10);
    }

    /// <summary>
    /// Verifies that validation succeeds when a raw connection string is provided.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_WhenConnectionStringIsProvided()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=test;"
        };

        var act = configuration.Validate;

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that validation fails when no connection information is provided.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenConnectionInfoIsMissing()
    {
        var configuration = new DatabaseOptions();

        var act = configuration.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Either ConnectionString or ConnectionOptions must be provided*");
    }

    /// <summary>
    /// Verifies that validation fails when both initialization modes are enabled.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenEnsureCreatedAndUseMigrationsAreEnabledTogether()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Data Source=test.db",
            EnsureCreated = true,
            UseMigrations = true
        };

        var act = configuration.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*EnsureCreated and UseMigrations cannot both be enabled*");
    }

    /// <summary>
    /// Verifies that validation fails when retry values are negative.
    /// </summary>
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void Validate_ShouldFail_WhenRetryValuesAreNegative(int retryCount, int retryDelaySeconds)
    {
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Data Source=test.db",
            RetryCount = retryCount,
            RetryDelaySeconds = retryDelaySeconds
        };

        var act = configuration.Validate;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that structured connection settings are validated through provider-agnostic rules.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenStructuredConnectionIsInvalid()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionOptions = new TestConnectionOptions()
        };

        var act = configuration.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Database connection requires a database name*");
    }

    /// <summary>
    /// Verifies that structured connection settings are accepted when valid.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_WhenStructuredConnectionIsValid()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionOptions = new TestConnectionOptions
            {
                Name = "test.db"
            }
        };

        var act = configuration.Validate;

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that a migrations history schema cannot be configured without a table name.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenMigrationsHistorySchemaHasNoTable()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Data Source=test.db",
            MigrationsHistorySchema = "application"
        };

        var act = configuration.Validate;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MigrationsHistoryTable must be configured*");
    }

    /// <summary>
    /// Verifies that context-specific migrations history settings are accepted together.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_WhenMigrationsHistoryTableAndSchemaAreConfigured()
    {
        var configuration = new DatabaseOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MigrationsHistoryTable = "__ApplicationMigrationsHistory",
            MigrationsHistorySchema = "application"
        };

        var act = configuration.Validate;

        act.Should().NotThrow();
    }

    private sealed class TestConnectionOptions : ConnectionOptions;
}
