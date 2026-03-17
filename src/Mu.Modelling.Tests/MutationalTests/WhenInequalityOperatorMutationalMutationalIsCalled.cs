namespace Mu.Modelling.MutationalTests;

public sealed class WhenInequalityOperatorMutationalMutationalIsCalled
{
    [Test]
    public void GivenDifferentValuesThenReturnsTrue()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational(fact: ModellingTestData.CreateAlternateName());

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        resultLeftRight.ShouldBeTrue();
        resultRightLeft.ShouldBeTrue();
    }

    [Test]
    public void GivenEqualValuesThenReturnsFalse()
    {
        // Arrange
        Mutational left = ModellingTestData.CreateMutational();
        Mutational right = ModellingTestData.CreateMutational();

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        resultLeftRight.ShouldBeFalse();
        resultRightLeft.ShouldBeFalse();
    }
}