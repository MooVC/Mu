namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        Feature.Kind left = Feature.Kind.Mutational;
        Feature.Kind right = Feature.Kind.NonMutational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsNotEqualTo(rightHash);
    }

    [Test]
    public async Task GivenSameValueThenHashesMatch()
    {
        // Arrange
        Feature.Kind left = Feature.Kind.Mutational;
        Feature.Kind right = Feature.Kind.Mutational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }
}