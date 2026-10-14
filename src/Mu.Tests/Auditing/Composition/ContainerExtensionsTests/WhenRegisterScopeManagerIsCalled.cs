namespace Mu.Auditing.Composition.ContainerExtensionsTests;

using SimpleInjector;

public sealed class WhenRegisterScopeManagerIsCalled
{
    [Test]
    public async Task GivenNullContainerThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The container used to register the scope manager must be provided.";
        Container container = default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterScopeManager());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(container));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}