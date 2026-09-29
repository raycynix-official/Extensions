using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.MySql.Options;

namespace Raycynix.Extensions.Database.MySql.Internal;

internal sealed class MySqlOptionsValidator : IConfigurationValidator<MySqlOptions>
{
    public ConfigurationValidationResult Validate(MySqlOptions options)
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