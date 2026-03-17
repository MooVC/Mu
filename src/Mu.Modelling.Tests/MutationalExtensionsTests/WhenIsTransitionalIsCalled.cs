namespace Mu.Modelling.MutationalExtensionsTests;

public sealed class WhenIsTransitionalIsCalled
{
    [Test]
    public async Task GivenMutationalThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();

        // Act
        Mutational result = original.IsTransitional();

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(Mutational.Kind.Transitional);
        await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}