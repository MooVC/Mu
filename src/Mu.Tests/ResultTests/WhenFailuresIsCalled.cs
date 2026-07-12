namespace Mu.ResultTests;

using Mu.Testing;

public sealed class WhenFailuresIsCalled
{
    [Test]
    public async Task GivenSuccessfulResultThenThrowsInvalidOperationException()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        Exception? exception = Capture(() => _ = subject.Failures);

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