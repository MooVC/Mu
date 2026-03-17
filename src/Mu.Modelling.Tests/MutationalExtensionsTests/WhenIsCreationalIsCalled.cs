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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(Mutational.Kind.Creational);
        await Assert.That(result.Fact).IsEqualTo(original.Fact);
    }
}