namespace Mu.Modelling.ModelNavigatorTests;

using Graphify;
using Microsoft.Extensions.DependencyInjection;

public sealed class WhenAddModelNavigatorIsCalled
{
    [Test]
    public async Task GivenServicesThenNavigatorIsRegistered()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        _ = services.AddModelNavigator();
        using ServiceProvider provider = services.BuildServiceProvider();
        INavigator<Model>? navigator = provider.GetService<INavigator<Model>>();

        // Assert
        _ = await Assert.That(navigator is not null).IsTrue();
    }
}