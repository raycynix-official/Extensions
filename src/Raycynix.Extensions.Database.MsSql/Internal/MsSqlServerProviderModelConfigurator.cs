using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.MsSql.Options;

namespace Raycynix.Extensions.Database.MsSql.Internal;

internal sealed class MsSqlServerProviderModelConfigurator(MsSqlServerOptions options)
    : IDatabaseProviderModelConfigurator
{
    private readonly string? _defaultSchema = options.DefaultSchema;

    public string ProviderName => "sqlserver";

    public string ModelCacheKey => System.Text.Json.JsonSerializer.Serialize(_defaultSchema);

    public void Configure(ModelBuilder modelBuilder)
    {
        if (_defaultSchema is not null)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);
        }
    }
}