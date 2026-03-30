namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenEqualityOperatorKindStringIsCalled
{
    private const string MutationalValue = "Mutational";
    private const string NonMutationalValue = "NonMutational";

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = NonMutationalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = MutationalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}
