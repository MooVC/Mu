namespace Mu.Modelling.RepresentationTests;

using Mu.Modelling.State;
using Mu.Testing;

public sealed class WhenImplicitOperatorFromTypeIsCalled
{
    [Test]
    public async Task GivenAggregateTypeThenReturnsRepresentation()
    {
        // Arrange
        Type type = typeof(MuTestData.TestAggregate);

        // Act
        Representation result = type;

        // Assert
        _ = await Assert.That(result.Assembly).IsEqualTo(type.Assembly.GetName().Name);
        _ = await Assert.That(result.Name).IsEqualTo(type.FullName![(type.Namespace!.Length + 1)..]);
        _ = await Assert.That(result.Namespace).IsEqualTo(type.Namespace);
    }

    [Test]
    public async Task GivenAbstractAggregateTypeThenThrowsArgumentException()
    {
        // Arrange
        Type type = typeof(AbstractAggregate);

        // Act
        Exception? exception = Capture(() =>
        {
            Representation result = type;

            _ = result;
        });

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
    }

    [Test]
    public async Task GivenNonAggregateTypeThenThrowsArgumentException()
    {
        // Arrange
        Type type = typeof(string);

        // Act
        Exception? exception = Capture(() =>
        {
            Representation result = type;

            _ = result;
        });

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
    }

    [Test]
    public async Task GivenNullTypeThenThrowsArgumentNullException()
    {
        // Arrange
        Type type = null!;

        // Act
        Exception? exception = Capture(() =>
        {
            Representation result = type;

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

    private abstract record AbstractAggregate
        : Aggregate;
}