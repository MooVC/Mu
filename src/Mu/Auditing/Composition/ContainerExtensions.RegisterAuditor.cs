namespace Mu.Auditing.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Mu.Auditing.Configuration;
using SimpleInjector;
using static Mu.Auditing.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        /// <summary>
        /// Registers an auditor using the audit options in the configuration.
        /// </summary>
        /// <param name="configuration">The source of audit options.</param>
        /// <returns>The container.</returns>
        public Container RegisterAuditor(IConfiguration configuration)
        {
            _ = Guard.Against.Null(configuration, message: RegisterAuditorConfigurationRequired);

            AuditOptions? options = configuration
                .GetSection(nameof(AuditOptions))
                .Get<AuditOptions>();

            return container.RegisterAuditor(options: options);
        }

        /// <summary>
        /// Registers an auditor using the supplied options or the defaults.
        /// </summary>
        /// <param name="options">The audit options.</param>
        /// <returns>The container.</returns>
        public Container RegisterAuditor(AuditOptions? options = default)
        {
            options ??= AuditOptions.Default;

            return options.Type switch
            {
                _ => container.RegisterInMemoryAuditor(),
            };
        }

        private Container RegisterInMemoryAuditor()
        {
            container.Register<IAuditor, InMemoryAuditor>(Lifestyle.Singleton);

            return container;
        }
    }
}