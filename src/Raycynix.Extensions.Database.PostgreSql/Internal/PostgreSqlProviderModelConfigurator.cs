using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.PostgreSql.Options;

namespace Raycynix.Extensions.Database.PostgreSql.Internal;

internal sealed class PostgreSqlProviderModelConfigurator(PostgreSqlOptions options)
    : IDatabaseProviderModelConfigurator
{
    private readonly string? _defaultSchema = options.DefaultSchema;

    public string ProviderName => "postgresql";

    public string ModelCacheKey => System.Text.Json.JsonSerializer.Serialize(_defaultSchema);

    public void Configure(ModelBuilder modelBuilder)
    {
        if (_defaultSchema is not null)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);
        }
    }
}
