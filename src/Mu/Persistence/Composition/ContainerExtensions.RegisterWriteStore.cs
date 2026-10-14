namespace Mu.Persistence.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Mu.Composition;
using Mu.Modelling.Services;
using Mu.Modelling.State;
using Mu.Persistence.Configuration;
using SimpleInjector;
using static Mu.Persistence.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterWriteStore<TAggregate, TIdentity>(IConfiguration configuration)
            where TAggregate : Aggregate, new()
            where TIdentity : struct
        {
            _ = Guard.Against.Null(container, message: RegisterWriteStoreContainerRequired);
            _ = Guard.Against.Null(configuration, message: RegisterWriteStoreConfigurationRequired);

            WriteStoreOptions? options = configuration
                .GetSection(nameof(WriteStoreOptions))
                .Get<WriteStoreOptions>();

            return container.RegisterWriteStore<TAggregate, TIdentity>(options: options);
        }

        public Container RegisterWriteStore<TAggregate, TIdentity>(WriteStoreOptions? options = default)
            where TAggregate : Aggregate, new()
            where TIdentity : struct
        {
            _ = Guard.Against.Null(container, message: RegisterWriteStoreContainerRequired);

            options ??= WriteStoreOptions.Default;

            return options.Type switch
            {
                _ => container.RegisterInMemoryWriteStore<TAggregate, TIdentity>(),
            };
        }

        private Container RegisterInMemoryWriteStore<TAggregate, TIdentity>()
            where TAggregate : Aggregate, new()
            where TIdentity : struct
        {
            _ = RegistrationFallbacks.Register<IStream<TIdentity>, InMemoryStream<TIdentity>>(container, Lifestyle.Singleton);
            _ = RegistrationFallbacks.Register<ITransform<TAggregate>, ReflectionTransform<TAggregate>>(container, Lifestyle.Scoped);

            return RegistrationFallbacks.Register<IWriteStore<TAggregate, TIdentity>, WriteStore<TAggregate, TIdentity>>(container, Lifestyle.Transient);
        }
    }
}