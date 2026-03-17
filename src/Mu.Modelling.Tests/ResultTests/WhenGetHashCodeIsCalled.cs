namespace Mu.Modelling.ResultTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenSameValuesThenHashesMatch()
    {
        // Arrange
        Result left = ModellingTestData.CreateResult();
        Result right = ModellingTestData.CreateResult();

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
        Result left = ModellingTestData.CreateResult();
        Result right = ModellingTestData.CreateResult(name: ModellingTestData.CreateAlternateName());

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
        Result subject = ModellingTestData.CreateResult();

        // Act
        int firstHash = subject.GetHashCode();
        int secondHash = subject.GetHashCode();

        // Assert
        await Assert.That(firstHash).IsEqualTo(secondHash);
    }
}