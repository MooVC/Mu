namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        Feature.Kinds left = Feature.Kinds.Mutational;
        Feature.Kinds right = Feature.Kinds.NonMutational;

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
        Feature.Kinds left = Feature.Kinds.Mutational;
        Feature.Kinds right = Feature.Kinds.Mutational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }
}