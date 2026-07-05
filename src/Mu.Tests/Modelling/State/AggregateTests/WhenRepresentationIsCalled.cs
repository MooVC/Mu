namespace Mu.Modelling.State.AggregateTests;

public sealed class WhenRepresentationIsCalled
{
    [Test]
    public async Task GivenAggregateBaseThenReturnsBaseRepresentation()
    {
        // Act
        Representation result = Aggregate.Representation;

        // Assert
        _ = await Assert.That(result.Name).IsEqualTo(nameof(Aggregate));
        _ = await Assert.That(result.Namespace).IsEqualTo(typeof(Aggregate).Namespace);
    }
}