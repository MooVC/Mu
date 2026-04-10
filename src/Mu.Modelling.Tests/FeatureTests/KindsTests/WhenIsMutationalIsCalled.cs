namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenIsMutationalIsCalled
{
    [Test]
    public async Task GivenMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNonMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.NonMutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}