namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

/// <summary>
/// Provides extensions for composing Mu applications.
/// </summary>
public static partial class IHostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Mu to the application composition root.
    /// </summary>
    /// <param name="root">The application composition root.</param>
    /// <returns>The configured dependency injection container.</returns>
    public static Container AddMu(this IHostApplicationBuilder root)
    {
        _ = Guard.Against.Null(root, message: "The application composition root must be provided.");

        _ = root.Services.AddMu(out Container container);

        return container;
    }
}