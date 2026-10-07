namespace Mu.Persistence.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
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
            container.Register<IWriteStore<TAggregate, TIdentity>, WriteStore<TAggregate, TIdentity>>(Lifestyle.Transient);

            return container;
        }
    }
}