namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

public static partial class IHostApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application host to use Mu with the specified dependency injection container.
    /// </summary>
    /// <param name="host">The application host.</param>
    /// <param name="container">The dependency injection container.</param>
    /// <returns>The configured application host.</returns>
    /// <exception cref="ArgumentNullException">Thrown when host or container are null references.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the dependency injection container is not properly configured.</exception>
    public static IHost UseMu(this IHost host, Container container)
    {
        _ = Guard.Against.Null(host, message: "The application host must be provided.");
        _ = Guard.Against.Null(container, message: "The dependency injection container must be provided.");

        host = host.UseSimpleInjector(container);

        container.Verify();

        return host;
    }
}