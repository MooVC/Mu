namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenIsNonMutationalIsCalled
{
    [Test]
    public async Task GivenMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenNonMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.NonMutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}