namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Mu.Auditing.Composition;
using Mu.Communications.Mediation.Composition;
using SimpleInjector;
using static Mu.Composition.IHostApplicationBuilderExtensions_Resources;

/// <summary>
/// Provides extensions for composing Mu applications.
/// </summary>
public static partial class IHostApplicationBuilderExtensions
{
    extension(IHostApplicationBuilder root)
    {
        /// <summary>
        /// Adds Mu to the application composition root.
        /// </summary>
        /// <param name="root">The application composition root.</param>
        /// <returns>The configured dependency injection container.</returns>
        public Container AddMu()
        {
            _ = Guard.Against.Null(root, message: AddMuRootRequired);

            _ = root.Services.AddMu(out Container container);

            return container
                .RegisterAuditor(root.Configuration)
                .RegisterMediator(root.Configuration);
        }
    }
}