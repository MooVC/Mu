namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using SimpleInjector;
using static Mu.Composition.WebApplicationBuilderExtensions_Resources;

public static partial class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Gets the associated Mu container without resolving services or changing the builder.
        /// </summary>
        /// <returns>The associated container, or <see langword="null"/> when Mu has not been added.</returns>
        public Container? GetMuContainer()
        {
            _ = Guard.Against.Null(builder, message: GetMuContainerBuilderRequired);

            return CompositionContext.Find(builder.Services)?.Container;
        }
    }
}