namespace Mu.Modelling.MutationalTests;

public sealed class WhenInequalityOperatorMutationalMutationalIsCalled
{
    [Test]
    public async Task GivenDifferentValuesThenReturnsTrue()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational(fact: ModellingTestData.CreateAlternateName());

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        await Assert.That(resultLeftRight).IsTrue();
        await Assert.That(resultRightLeft).IsTrue();
    }

    [Test]
    public async Task GivenEqualValuesThenReturnsFalse()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational();

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        await Assert.That(resultLeftRight).IsFalse();
        await Assert.That(resultRightLeft).IsFalse();
    }
}