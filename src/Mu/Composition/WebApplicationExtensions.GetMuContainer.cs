namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using static Mu.Composition.WebApplicationExtensions_Resources;

public static partial class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Gets the associated Mu container without resolving application services or locking registrations.
        /// </summary>
        /// <returns>The associated container, or <see langword="null"/> when Mu has not been added.</returns>
        public Container? GetMuContainer()
        {
            _ = Guard.Against.Null(app, message: GetMuContainerAppRequired);

            return app.Services.GetService<CompositionContext>()?.Container;
        }
    }
}