namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenEqualityOperatorKindStringIsCalled
{
    private const string MutationalValue = "Mutational";
    private const string NonMutationalValue = "NonMutational";

    [Test]
    public void GivenMatchingValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = MutationalValue;

        // Act
        bool result = subject == value;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = NonMutationalValue;

        // Act
        bool result = subject == value;

        // Assert
        result.ShouldBeFalse();
    }
}