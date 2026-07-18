namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using SimpleInjector.Lifestyles;

/// <summary>
/// Provides extensions for composing Mu applications.
/// </summary>
public static class IHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Mu to the application composition root.
    /// </summary>
    /// <param name="root">The application composition root.</param>
    /// <returns>The configured dependency injection container.</returns>
    public static Container AddMu(this IHostApplicationBuilder root)
    {
        _ = Guard.Against.Null(root, message: "The application composition root must be provided.");

        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        _ = root.Services.AddSimpleInjector(container, options => options.AddLogging());

        return container;
    }
}