namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenHashesDiffer()
    {
        // Arrange
        NonMutational.Kinds left = NonMutational.Kinds.ReadStore;
        NonMutational.Kinds right = NonMutational.Kinds.WriteStore;

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
        NonMutational.Kinds left = NonMutational.Kinds.ReadStore;
        NonMutational.Kinds right = NonMutational.Kinds.ReadStore;

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }
}