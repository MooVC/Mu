namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenInequalityOperatorKindStringIsCalled
{
    private const string MutationalValue = "Mutational";
    private const string NonMutationalValue = "NonMutational";

    [Test]
    public async Task GivenDifferentValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = NonMutationalValue;

        // Act
        bool result = subject != value;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;
        string value = MutationalValue;

        // Act
        bool result = subject != value;

        // Assert
        await Assert.That(result).IsFalse();
    }
}