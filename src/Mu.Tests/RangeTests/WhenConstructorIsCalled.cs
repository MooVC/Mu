namespace Mu.RangeTests;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenAscendingBoundsThenPropertiesAreAssigned()
    {
        // Arrange
        const int from = 1;
        const int to = 3;

        // Act
        var result = new Range<int>(from, to);

        // Assert
        _ = await Assert.That(result.From).IsEqualTo(from);
        _ = await Assert.That(result.To).IsEqualTo(to);
    }

    [Test]
    public async Task GivenEqualBoundsThenPropertiesAreAssigned()
    {
        // Arrange
        const int bound = 3;

        // Act
        var result = new Range<int>(bound, bound);

        // Assert
        _ = await Assert.That(result.From).IsEqualTo(bound);
        _ = await Assert.That(result.To).IsEqualTo(bound);
    }

    [Test]
    public async Task GivenDescendingBoundsThenThrowsArgumentException()
    {
        // Arrange
        const int from = 3;
        const int to = 1;
        const string expectedMessage = "The From value of `3` must be lower than the To value To `1`.";

        // Act
        Exception? exception = Capture(() => _ = new Range<int>(from, to));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
        _ = await Assert.That(((ArgumentException)exception!).ParamName).IsEqualTo(nameof(to));
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