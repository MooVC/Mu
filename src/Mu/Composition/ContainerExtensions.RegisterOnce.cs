namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using static Mu.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    private static readonly object _registrars = new();

    extension(Container container)
    {
        /// <summary>
        /// Runs a registrar once for the container, recording it only after registration succeeds.
        /// </summary>
        /// <typeparam name="TRegistrar">The registrar to run.</typeparam>
        /// <param name="configuration">The source of configuration values.</param>
        /// <returns>The container.</returns>
        public Container RegisterOnce<TRegistrar>(IConfiguration configuration)
            where TRegistrar : IRegistrar
        {
            _ = Guard.Against.Null(container, message: RegisterOnceContainerRequired);
            _ = Guard.Against.Null(configuration, message: RegisterOnceConfigurationRequired);

            HashSet<Type> registrars = container.ContainerScope.GetOrSetItem(_registrars, static (_, _) => new HashSet<Type>());

            if (!registrars.Contains(typeof(TRegistrar)))
            {
                TRegistrar.Register(configuration, container);
                _ = registrars.Add(typeof(TRegistrar));
            }

            return container;
        }
    }
}