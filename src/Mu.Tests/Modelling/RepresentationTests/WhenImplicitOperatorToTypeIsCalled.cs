namespace Mu.Modelling.RepresentationTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToTypeIsCalled
{
    [Test]
    public async Task GivenRepresentationThenReturnsType()
    {
        // Arrange
        Representation subject = typeof(MuTestData.TestAggregate);

        // Act
        Type result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(typeof(MuTestData.TestAggregate));
    }

    [Test]
    public async Task GivenNullRepresentationThenThrowsArgumentNullException()
    {
        // Arrange
        Representation subject = null!;

        // Act
        Exception? exception = Capture(() =>
        {
            Type result = subject;

            _ = result;
        });

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