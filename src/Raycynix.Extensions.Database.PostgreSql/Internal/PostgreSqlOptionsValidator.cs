using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.PostgreSql.Options;

namespace Raycynix.Extensions.Database.PostgreSql.Internal;

internal sealed class PostgreSqlOptionsValidator : IConfigurationValidator<PostgreSqlOptions>
{
    public ConfigurationValidationResult Validate(PostgreSqlOptions options)
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