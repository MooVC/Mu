namespace Mu.Communications.Mediation.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Mu.Communications.Mediation.Configuration;
using SimpleInjector;
using static Mu.Communications.Mediation.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        /// <summary>
        /// Registers a mediator using the mediation options in the configuration.
        /// </summary>
        /// <param name="configuration">The source of mediation options.</param>
        /// <returns>The container.</returns>
        public Container RegisterMediator(IConfiguration configuration)
        {
            _ = Guard.Against.Null(configuration, message: RegisterMediatorConfigurationRequired);

            MediationOptions? options = configuration
                .GetSection(nameof(MediationOptions))
                .Get<MediationOptions>();

            return container.RegisterMediator(options: options);
        }

        /// <summary>
        /// Registers a mediator using the supplied options or the defaults.
        /// </summary>
        /// <param name="options">The mediation options.</param>
        /// <returns>The container.</returns>
        public Container RegisterMediator(MediationOptions? options = default)
        {
            options ??= MediationOptions.Default;

            return options.Type switch
            {
                _ => container.RegisterInMemoryMediator(),
            };
        }

        private Container RegisterInMemoryMediator()
        {
            container.Register<IMediator, InMemoryMediator>(Lifestyle.Scoped);

            return container;
        }
    }
}