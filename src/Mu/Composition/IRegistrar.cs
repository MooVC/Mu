namespace Mu.Composition;

using Microsoft.Extensions.Configuration;
using SimpleInjector;

/// <summary>
/// Defines a contract for types that can register themselves in a dependency injection container.
/// </summary>
public interface IRegistrar
{
    /// <summary>
    /// Registers the implementing type in the provided container.
    /// </summary>
    /// <param name="configuration">The source of configuration values used to direct registration.</param>
    /// <param name="container">The dependency injection container.</param>
    static abstract void Register(IConfiguration configuration, Container container);
}