namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenEqualityOperatorKindsStringIsCalled
{
    private const string MutationalValue = "Mutational";
    private const string NonMutationalValue = "NonMutational";

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;
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
        Feature.Kinds subject = Feature.Kinds.Mutational;
        string value = MutationalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}