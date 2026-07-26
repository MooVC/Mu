namespace Mu.Composition.IServiceCollectionExtensionsTests;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed class WhenAddMuIsCalled
{
    [Test]
    public async Task GivenACompositionRootThenSimpleInjectorIsConfigured()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();

        // Act
        _ = root.Services.AddMu(out Container container);
        container.Register<Subject>();

        using IHost host = root
            .Build()
            .UseSimpleInjector(container);

        container.Verify();
        Subject subject = container.GetInstance<Subject>();

        // Assert
        _ = await Assert.That(container.Options.DefaultScopedLifestyle).IsTypeOf<AsyncScopedLifestyle>();
        _ = await Assert.That(subject.Logger).IsNotNull();
    }

    private sealed class Subject(ILogger logger)
    {
        public ILogger Logger { get; } = logger;
    }
}