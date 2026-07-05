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
        // Act
        Exception? exception = Capture(() => _ = new Range<int>(3, 1));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
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