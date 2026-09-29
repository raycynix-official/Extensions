using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides the default EF Core database context used by the Raycynix database infrastructure.
/// </summary>
public sealed class RaycynixDatabaseContext : DbContext, IRaycynixDatabaseContext
{
    private readonly IDatabaseContextServices<RaycynixDatabaseContext> _services;

    /// <summary>
    /// Initializes a new instance of <see cref="RaycynixDatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="services">The infrastructure isolated for this context.</param>
    public RaycynixDatabaseContext(
        DbContextOptions options,
        IDatabaseContextServices<RaycynixDatabaseContext> services)
        : base(options)
    {
        _services = services;

        ConfigureChangeTracker();
    }

    /// <summary>
    /// Applies configurators from the registered model assemblies and optionally registers seed data.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _services.ConfigureModel(builder);
    }

    private void ConfigureChangeTracker()
    {
        ChangeTracker.LazyLoadingEnabled = _services.Options.EnableLazyLoading;
        ChangeTracker.AutoDetectChangesEnabled = _services.Options.EnableAutoDetectChanges;
        ChangeTracker.QueryTrackingBehavior = _services.Options.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _services.GetModelCacheKey();
    }
}