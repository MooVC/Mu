namespace Mu.Modelling.ModelNavigatorTests;

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
        IModelNavigator? navigator = provider.GetService<IModelNavigator>();

        // Assert
        _ = await Assert.That(navigator).IsNotNull();
    }
}