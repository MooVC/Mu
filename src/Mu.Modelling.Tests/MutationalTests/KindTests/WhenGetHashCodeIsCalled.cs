namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public void GivenSameValueThenHashesMatch()
    {
        // Arrange
        Mutational.Kind left = Mutational.Kind.Creational;
        Mutational.Kind right = Mutational.Kind.Creational;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        leftHash.ShouldBe(rightHash);
    }

    [Test]
    public void GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        Mutational.Kind left = Mutational.Kind.Creational;
        Mutational.Kind right = Mutational.Kind.Transitional;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        leftHash.ShouldNotBe(rightHash);
    }
}