namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        Mutational.Kinds left = Mutational.Kinds.Creational;
        Mutational.Kinds right = Mutational.Kinds.Transitional;

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
        Mutational.Kinds left = Mutational.Kinds.Creational;
        Mutational.Kinds right = Mutational.Kinds.Creational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }
}