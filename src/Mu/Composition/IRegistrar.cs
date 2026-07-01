namespace Mu.Composition;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Defines a contract for types that can register themselves in a dependency injection container.
/// </summary>
/// <typeparam name="T">The type of the dependency injection container.</typeparam>
public interface IRegistrar<T>
    where T : class
{
    /// <summary>
    /// Registers the implementing type in the provided container.
    /// </summary>
    /// <param name="configuration">The source of configuration values used to direct registration.</param>
    /// <param name="container">The dependency injection container.</param>
    /// <returns>The updated dependency injection container.</returns>
    static abstract T Register(IConfiguration configuration, T container);
}