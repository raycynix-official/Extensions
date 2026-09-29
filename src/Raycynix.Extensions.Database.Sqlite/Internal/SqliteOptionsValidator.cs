using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.Sqlite.Options;

namespace Raycynix.Extensions.Database.Sqlite.Internal;

internal sealed class SqliteOptionsValidator : IConfigurationValidator<SqliteOptions>
{
    public ConfigurationValidationResult Validate(SqliteOptions options)
    {
        try
        {
            options.Validate();
            return ConfigurationValidationResult.Success();
        }
        catch (Exception exception)
        {
            return ConfigurationValidationResult.Failure(exception.Message);
        }
    }
}