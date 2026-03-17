namespace Mu.Modelling.MutationalTests;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();

        // Act
        Mutational result = original.OfType(Mutational.Kind.Transitional);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(Mutational.Kind.Transitional);
        await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}