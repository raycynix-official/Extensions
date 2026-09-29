namespace Raycynix.Extensions.Database.Abstractions.Configurators;

/// <summary>
/// Defines a typed configurator for an entity.
/// </summary>
/// <typeparam name="T">The entity type handled by the configurator.</typeparam>
public interface IGenericConfigurator<T> : IConfigurator where T : class
{
}