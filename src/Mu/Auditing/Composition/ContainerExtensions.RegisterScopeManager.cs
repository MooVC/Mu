namespace Mu.Auditing.Composition;

using Ardalis.GuardClauses;
using SimpleInjector;
using static Mu.Auditing.Composition.ContainerExtensions_Resources;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterScopeManager()
        {
            _ = Guard.Against.Null(container, message: RegisterScopeManagerContainerRequired);

            container.RegisterConditional<IScopeManager, ScopeManager>(Lifestyle.Singleton, context => !context.Handled);

            return container;
        }
    }
}