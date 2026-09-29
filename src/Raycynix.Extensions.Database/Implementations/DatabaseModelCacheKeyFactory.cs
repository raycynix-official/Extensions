using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Builds EF Core model cache keys that include the active provider, seed mode, and configurator model shape.
/// </summary>
internal sealed class DatabaseModelCacheKeyFactory : IModelCacheKeyFactory
{
    /// <inheritdoc />
    public object Create(DbContext context, bool designTime)
    {
        if (context is not IRaycynixDatabaseContext databaseContext)
        {
            return (context.GetType(), designTime);
        }

        return (context.GetType(), databaseContext.GetModelCacheKey(), designTime);
    }
}