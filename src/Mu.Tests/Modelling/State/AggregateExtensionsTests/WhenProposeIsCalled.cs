namespace Mu.Modelling.State.AggregateExtensionsTests;

using Mu.Testing;

public sealed class WhenProposeIsCalled
{
    [Test]
    public async Task GivenFactThenAppendsPropositionAndAppliesTransforms()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        var fact = new MuTestData.TestFact();
        var transform = new MuTestData.TestTransform();

        // Act
        MuTestData.TestAggregate result = aggregate.Propose(fact, transform);

        // Assert
        _ = await Assert.That(result.Propositions).IsEquivalentTo(new[] { fact });
        _ = await Assert.That(result.Value).IsEqualTo(aggregate.Value + fact.Value);
        _ = await Assert.That(transform.Facts).IsEquivalentTo(new[] { fact });
        _ = await Assert.That(aggregate.Propositions).IsEmpty();
    }

    [Test]
    public async Task GivenNullAggregateThenThrowsArgumentNullException()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = null!;
        var fact = new MuTestData.TestFact();

        // Act
        Exception? exception = Capture(() => _ = aggregate.Propose(fact));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenNullFactThenThrowsArgumentNullException()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        MuTestData.TestFact fact = null!;

        // Act
        Exception? exception = Capture(() => _ = aggregate.Propose(fact));

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