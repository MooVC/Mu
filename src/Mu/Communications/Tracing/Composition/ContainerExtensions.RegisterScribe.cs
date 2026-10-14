namespace Mu.Communications.Tracing.Composition;

using Ardalis.GuardClauses;
using SimpleInjector;
using static Mu.Communications.Tracing.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterScribe()
        {
            _ = Guard.Against.Null(container, message: RegisterScribeContainerRequired);

            container.RegisterConditional<IScribe, Scribe>(Lifestyle.Singleton, context => !context.Handled);

            return container;
        }
    }
}