using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Initializes the messaging persistence schema during application startup.
/// </summary>
internal sealed class MessagingDatabasePersistenceInitializationService(
    IEnumerable<IDatabaseInitializer> databaseInitializers) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var databaseInitializer in databaseInitializers)
        {
            await databaseInitializer.InitializeAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}