namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public void GivenSameValueThenHashesMatch()
    {
        // Arrange
        NonMutational.Kind left = NonMutational.Kind.ReadStore;
        NonMutational.Kind right = NonMutational.Kind.ReadStore;

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
        NonMutational.Kind left = NonMutational.Kind.ReadStore;
        NonMutational.Kind right = NonMutational.Kind.WriteStore;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        leftHash.ShouldNotBe(rightHash);
    }
}