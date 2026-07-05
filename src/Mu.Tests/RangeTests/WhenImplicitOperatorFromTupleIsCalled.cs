namespace Mu.RangeTests;

public sealed class WhenImplicitOperatorFromTupleIsCalled
{
    [Test]
    public async Task GivenTupleThenReturnsRange()
    {
        // Arrange
        const int from = 1;
        const int to = 3;

        // Act
        Range<int> result = (from, to);

        // Assert
        _ = await Assert.That(result.From).IsEqualTo(from);
        _ = await Assert.That(result.To).IsEqualTo(to);
    }
}