namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using static Mu.Composition.IApplicationBuilderExtensions_Resources;

public static partial class IApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application host to use Mu with the specified dependency injection container.
    /// </summary>
    /// <param name="host">The application host.</param>
    /// <param name="container">The dependency injection container.</param>
    /// <returns>The configured application host.</returns>
    /// <exception cref="ArgumentNullException">Thrown when host or container are null references.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the dependency injection container is not properly configured.</exception>
    public static IApplicationBuilder UseMu(this IApplicationBuilder host, Container container, Action<SimpleInjectorUseOptions>? options = default)
    {
        _ = Guard.Against.Null(host, message: UseMuHostRequired);
        _ = Guard.Against.Null(container, message: UseMuContainerRequired);

        options ??= _ => { };

        host = host
            .UseSimpleInjector(container, options)
            .Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next(context);
                }
            });

        container.Verify();

        return host;
    }
}