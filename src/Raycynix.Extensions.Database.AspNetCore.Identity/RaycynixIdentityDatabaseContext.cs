using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.AspNetCore.Identity;

/// <summary>
/// Provides the default ASP.NET Core Identity database context used by the Raycynix database infrastructure.
/// </summary>
public sealed class RaycynixIdentityDatabaseContext : IdentityDbContext, IRaycynixIdentityDatabaseContext
{
    private readonly RaycynixIdentityDatabaseContextServices _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaycynixIdentityDatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="services">The infrastructure isolated for this context.</param>
    public RaycynixIdentityDatabaseContext(
        DbContextOptions<RaycynixIdentityDatabaseContext> options,
        IDatabaseContextServices<RaycynixIdentityDatabaseContext> services)
        : base(options)
    {
        _services = new RaycynixIdentityDatabaseContextServices(
            services.Options, services.ConfigureModel, services.GetModelCacheKey);
        _services.ConfigureChangeTracker(this);
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

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _services.GetModelCacheKey();
    }
}

/// <summary>
/// Provides an ASP.NET Core Identity database context for custom user entities.
/// </summary>
/// <typeparam name="TUser">The user entity type.</typeparam>
public sealed class RaycynixIdentityDatabaseContext<TUser> : IdentityDbContext<TUser>, IRaycynixIdentityDatabaseContext
    where TUser : IdentityUser
{
    private readonly RaycynixIdentityDatabaseContextServices _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaycynixIdentityDatabaseContext{TUser}"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="services">The infrastructure isolated for this context.</param>
    public RaycynixIdentityDatabaseContext(
        DbContextOptions<RaycynixIdentityDatabaseContext<TUser>> options,
        IDatabaseContextServices<RaycynixIdentityDatabaseContext<TUser>> services)
        : base(options)
    {
        _services = new RaycynixIdentityDatabaseContextServices(
            services.Options, services.ConfigureModel, services.GetModelCacheKey);
        _services.ConfigureChangeTracker(this);
    }

    /// <summary>
    /// Applies ASP.NET Core Identity mappings and registered Raycynix model configurators.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _services.ConfigureModel(builder);
    }

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _services.GetModelCacheKey();
    }
}

/// <summary>
/// Provides an ASP.NET Core Identity database context for custom user, role, and key types.
/// </summary>
/// <typeparam name="TUser">The user entity type.</typeparam>
/// <typeparam name="TRole">The role entity type.</typeparam>
/// <typeparam name="TKey">The primary key type used by Identity entities.</typeparam>
public sealed class RaycynixIdentityDatabaseContext<TUser, TRole, TKey>
    : IdentityDbContext<TUser, TRole, TKey>, IRaycynixIdentityDatabaseContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly RaycynixIdentityDatabaseContextServices _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaycynixIdentityDatabaseContext{TUser, TRole, TKey}"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="services">The infrastructure isolated for this context.</param>
    public RaycynixIdentityDatabaseContext(
        DbContextOptions<RaycynixIdentityDatabaseContext<TUser, TRole, TKey>> options,
        IDatabaseContextServices<RaycynixIdentityDatabaseContext<TUser, TRole, TKey>> services)
        : base(options)
    {
        _services = new RaycynixIdentityDatabaseContextServices(
            services.Options, services.ConfigureModel, services.GetModelCacheKey);
        _services.ConfigureChangeTracker(this);
    }

    /// <summary>
    /// Applies ASP.NET Core Identity mappings and registered Raycynix model configurators.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _services.ConfigureModel(builder);
    }

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _services.GetModelCacheKey();
    }
}

/// <summary>
/// Provides an ASP.NET Core Identity database context for fully customized Identity entity types.
/// </summary>
/// <typeparam name="TUser">The user entity type.</typeparam>
/// <typeparam name="TRole">The role entity type.</typeparam>
/// <typeparam name="TKey">The primary key type used by Identity entities.</typeparam>
/// <typeparam name="TUserClaim">The user claim entity type.</typeparam>
/// <typeparam name="TUserRole">The user role entity type.</typeparam>
/// <typeparam name="TUserLogin">The user login entity type.</typeparam>
/// <typeparam name="TRoleClaim">The role claim entity type.</typeparam>
/// <typeparam name="TUserToken">The user token entity type.</typeparam>
public sealed class RaycynixIdentityDatabaseContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
    TUserToken>
    : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>,
        IRaycynixIdentityDatabaseContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
{
    private readonly RaycynixIdentityDatabaseContextServices _services;

    /// <summary>
    /// Initializes a new instance of the fully customized Raycynix Identity database context.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="services">The infrastructure isolated for this context.</param>
    public RaycynixIdentityDatabaseContext(
        DbContextOptions<RaycynixIdentityDatabaseContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin,
            TRoleClaim, TUserToken>> options,
        IDatabaseContextServices<RaycynixIdentityDatabaseContext<TUser, TRole, TKey, TUserClaim, TUserRole,
            TUserLogin, TRoleClaim, TUserToken>> services)
        : base(options)
    {
        _services = new RaycynixIdentityDatabaseContextServices(
            services.Options, services.ConfigureModel, services.GetModelCacheKey);
        _services.ConfigureChangeTracker(this);
    }

    /// <summary>
    /// Applies ASP.NET Core Identity mappings and registered Raycynix model configurators.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _services.ConfigureModel(builder);
    }

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _services.GetModelCacheKey();
    }
}

internal sealed class RaycynixIdentityDatabaseContextServices(
    DatabaseOptions config,
    Action<ModelBuilder> configureModel,
    Func<string> getModelCacheKey)
{
    public void ConfigureChangeTracker(DbContext context)
    {
        context.ChangeTracker.LazyLoadingEnabled = config.EnableLazyLoading;
        context.ChangeTracker.AutoDetectChangesEnabled = config.EnableAutoDetectChanges;
        context.ChangeTracker.QueryTrackingBehavior = config.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    public void ConfigureModel(ModelBuilder builder)
    {
        configureModel(builder);
    }

    public string GetModelCacheKey()
    {
        return getModelCacheKey();
    }
}