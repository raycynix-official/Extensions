using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Hosting.Tests;

/// <summary>
/// Covers public initialization extensions for generic host-based applications.
/// </summary>
public sealed class DatabaseInitializationTests
{
    /// <summary>
    /// Verifies that initialization resolves <see cref="IDatabaseInitializer"/> from a scoped service provider.
    /// </summary>
    [Fact]
    public async Task InitializeRaycynixDatabaseAsync_ForServiceProvider_ShouldInvokeInitializer()
    {
        var initializer = new FakeDatabaseInitializer();
        var services = new ServiceCollection();
        services.AddScoped<IDatabaseInitializer>(_ => initializer);

        await using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        await serviceProvider.InitializeRaycynixDatabaseAsync(TestContext.Current.CancellationToken);

        initializer.CallCount.Should().Be(1);
        initializer.LastCancellationToken.Should().Be(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Verifies that every registered database context initializer is invoked.
    /// </summary>
    [Fact]
    public async Task InitializeRaycynixDatabaseAsync_WithMultipleInitializers_ShouldInvokeAll()
    {
        var firstInitializer = new FakeDatabaseInitializer();
        var secondInitializer = new FakeDatabaseInitializer();
        var services = new ServiceCollection();
        services.AddScoped<IDatabaseInitializer>(_ => firstInitializer);
        services.AddScoped<IDatabaseInitializer>(_ => secondInitializer);

        await using var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        await serviceProvider.InitializeRaycynixDatabaseAsync(TestContext.Current.CancellationToken);

        firstInitializer.CallCount.Should().Be(1);
        secondInitializer.CallCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that host-based initialization delegates to the host service provider.
    /// </summary>
    [Fact]
    public async Task InitializeRaycynixDatabaseAsync_ForHost_ShouldInvokeInitializer()
    {
        var initializer = new FakeDatabaseInitializer();
        using var host = new HostBuilder()
            .ConfigureServices(services => { services.AddScoped<IDatabaseInitializer>(_ => initializer); })
            .Build();

        await host.InitializeRaycynixDatabaseAsync(TestContext.Current.CancellationToken);

        initializer.CallCount.Should().Be(1);
        initializer.LastCancellationToken.Should().Be(TestContext.Current.CancellationToken);
    }

    private sealed class FakeDatabaseInitializer : IDatabaseInitializer
    {
        public int CallCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public bool IsReady => CallCount > 0;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}