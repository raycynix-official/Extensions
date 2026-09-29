using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.MsSql.Options;

namespace Raycynix.Extensions.Database.MsSql.Internal;

internal sealed class MsSqlServerOptionsValidator : IConfigurationValidator<MsSqlServerOptions>
{
    public ConfigurationValidationResult Validate(MsSqlServerOptions options)
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