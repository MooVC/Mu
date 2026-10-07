namespace Mu.Auditing.Composition;

using SimpleInjector;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterScopeManager()
        {
            container.RegisterConditional<IScopeManager, ScopeManager>(Lifestyle.Singleton, context => !context.Handled);

            return container;
        }
    }
}