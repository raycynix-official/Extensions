using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.Abstractions.Options;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Adapts <see cref="DatabaseOptions.Validate"/> to the Raycynix configuration validation pipeline.
/// </summary>
internal sealed class DatabaseOptionsValidator : IConfigurationValidator<DatabaseOptions>
{
    /// <summary>
    /// Validates the bound database configuration and returns a structured validation result.
    /// </summary>
    /// <param name="options">The database configuration to validate.</param>
    /// <returns>The validation result for the provided configuration.</returns>
    public ConfigurationValidationResult Validate(DatabaseOptions options)
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