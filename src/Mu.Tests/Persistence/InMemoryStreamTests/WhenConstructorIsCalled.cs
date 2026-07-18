namespace Mu.Persistence.InMemoryStreamTests;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenNullSemaphoreThenThrowsArgumentNullException()
    {
        // Arrange
        SemaphoreSlim semaphore = null!;

        // Act
        Exception? exception = Capture(() => _ = new InMemoryStream<Guid>(semaphore));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
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