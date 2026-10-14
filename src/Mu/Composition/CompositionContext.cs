namespace Mu.Composition;

using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

internal sealed class CompositionContext(Container container)
{
    public Container Container { get; } = container;

    public SimpleInjectorAddOptions? IntegrationOptions { get; set; }

    public IServiceProvider? ServiceProvider { get; set; }

    public bool VerifyOnStartup { get; set; }

    public static CompositionContext? Find(IServiceCollection services)
    {
        return services
            .Select(service => service.ImplementationInstance)
            .OfType<CompositionContext>()
            .FirstOrDefault();
    }
}