namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenIsMutationalIsCalled
{
    [Test]
    public void GivenMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNonMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        bool result = subject.IsMutational;

        // Assert
        result.ShouldBeFalse();
    }
}