namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenIsNonMutationalIsCalled
{
    [Test]
    public void GivenNonMutationalKindThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenMutationalKindThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        bool result = subject.IsNonMutational;

        // Assert
        result.ShouldBeFalse();
    }
}