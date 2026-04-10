namespace Mu.Modelling.MutationalTests;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();

        // Act
        Mutational result = original.OfType(Mutational.Kinds.Transitional);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Type).IsEqualTo(Mutational.Kinds.Transitional);
        _ = await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}