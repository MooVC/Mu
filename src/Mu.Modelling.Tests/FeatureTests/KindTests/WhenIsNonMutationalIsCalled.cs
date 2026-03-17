namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenIsNonMutationalIsCalled
{
    [Test]
    public async Task GivenNonMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        await Assert.That(result).IsFalse();
    }
}