namespace Mu.Auditing.Composition;

using SimpleInjector;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        public Container RegisterScopeManager()
        {
            container.Register<IScopeManager, ScopeManager>(Lifestyle.Singleton);

            return container;
        }
    }
}