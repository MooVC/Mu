namespace Mu.Communications.Tracing.Composition.ContainerExtensionsTests;

using SimpleInjector;

public sealed class WhenRegisterScribeIsCalled
{
    [Test]
    public async Task GivenNullContainerThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The container used to register the scribe must be provided.";
        Container container = default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterScribe());

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