using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Initializes the messaging persistence schema during application startup.
/// </summary>
internal sealed class MessagingDatabasePersistenceInitializationService(
    IDatabaseInitializer<RaycynixDatabaseContext> databaseInitializer) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await databaseInitializer.InitializeAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}