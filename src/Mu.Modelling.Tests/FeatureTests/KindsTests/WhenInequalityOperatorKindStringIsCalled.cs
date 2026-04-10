namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenInequalityOperatorKindStringIsCalled
{
    private const string MutationalValue = "Mutational";
    private const string NonMutationalValue = "NonMutational";

    [Test]
    public async Task GivenDifferentValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;
        string value = NonMutationalValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;
        string value = MutationalValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}