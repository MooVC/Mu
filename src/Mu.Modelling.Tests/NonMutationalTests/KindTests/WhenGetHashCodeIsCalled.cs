namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenSameValueThenHashesMatch()
    {
        // Arrange
        NonMutational.Kind left = NonMutational.Kind.ReadStore;
        NonMutational.Kind right = NonMutational.Kind.ReadStore;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        await Assert.That(leftHash).IsEqualTo(rightHash);
    }

    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        NonMutational.Kind left = NonMutational.Kind.ReadStore;
        NonMutational.Kind right = NonMutational.Kind.WriteStore;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        await Assert.That(leftHash).IsNotEqualTo(rightHash);
    }
}