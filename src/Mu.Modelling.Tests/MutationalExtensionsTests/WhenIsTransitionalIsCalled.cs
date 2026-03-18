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
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Type).IsEqualTo(Mutational.Kind.Transitional);
        _ = await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}