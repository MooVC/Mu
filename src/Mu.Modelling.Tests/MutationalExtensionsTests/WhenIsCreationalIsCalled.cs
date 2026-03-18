namespace Mu.Modelling.MutationalExtensionsTests;

public sealed class WhenIsCreationalIsCalled
{
    [Test]
    public async Task GivenMutationalThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();

        // Act
        Mutational result = original.IsCreational();

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Type).IsEqualTo(Mutational.Kind.Creational);
        _ = await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}