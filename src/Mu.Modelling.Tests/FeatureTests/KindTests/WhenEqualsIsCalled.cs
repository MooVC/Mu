namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenSameValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        Feature.Kind other = Feature.Kind.Mutational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        Feature.Kind other = Feature.Kind.NonMutational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        result.ShouldBeFalse();
    }
}