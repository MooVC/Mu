namespace Mu.Modelling.MutationalTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenSameValuesThenHashesMatch()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational();

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        await Assert.That(leftHash).IsEqualTo(rightHash);
    }

    [Test]
    public async Task GivenDifferentValuesThenHashesDiffer()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational(fact: ModellingTestData.CreateAlternateName());

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        await Assert.That(leftHash).IsNotEqualTo(rightHash);
    }

    [Test]
    public async Task GivenSameInstanceThenHashIsStable()
    {
        // Arrange
        Mutational subject = ModellingTestData.CreateMutational();

        // Act
        int firstHash = subject.GetHashCode();
        int secondHash = subject.GetHashCode();

        // Assert
        await Assert.That(firstHash).IsEqualTo(secondHash);
    }
}