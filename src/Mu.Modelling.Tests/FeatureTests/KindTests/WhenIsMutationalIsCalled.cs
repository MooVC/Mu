namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenIsMutationalIsCalled
{
    [Test]
    public async Task GivenMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNonMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}