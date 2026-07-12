namespace Mu.ResultTests;

using Mu.Testing;

public sealed class WhenValueIsCalled
{
    [Test]
    public async Task GivenUnsuccessfulResultThenThrowsInvalidOperationException()
    {
        // Arrange
        Result<string> subject = TestData.CreateFailure();

        // Act
        Exception? exception = Capture(() => _ = subject.Value);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
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