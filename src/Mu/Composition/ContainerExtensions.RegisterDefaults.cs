namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Mu.Auditing;
using Mu.Communications.Mediation;
using SimpleInjector;
using static Mu.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        /// <summary>
        /// Supplies the configured shared services when an application has not registered them.
        /// </summary>
        /// <param name="configuration">The source of configuration values.</param>
        /// <returns>The container.</returns>
        public Container RegisterDefaults(IConfiguration configuration)
        {
            _ = Guard.Against.Null(container, message: RegisterDefaultsContainerRequired);
            _ = Guard.Against.Null(configuration, message: RegisterDefaultsConfigurationRequired);

            _ = RegistrationFallbacks.Register<IAuditor, InMemoryAuditor>(container, Lifestyle.Singleton);

            return RegistrationFallbacks.Register<IMediator, InMemoryMediator>(container, Lifestyle.Scoped);
        }
    }
}