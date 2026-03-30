namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        Mutational.Kind left = Mutational.Kind.Creational;
        Mutational.Kind right = Mutational.Kind.Transitional;

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
        Mutational.Kind left = Mutational.Kind.Creational;
        Mutational.Kind right = Mutational.Kind.Creational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }
}