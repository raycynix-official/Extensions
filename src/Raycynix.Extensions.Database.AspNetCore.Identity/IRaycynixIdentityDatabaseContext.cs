using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.AspNetCore.Identity;

/// <summary>
/// Marks a Raycynix database context as an ASP.NET Core Identity context.
/// </summary>
public interface IRaycynixIdentityDatabaseContext : IRaycynixDatabaseContext;