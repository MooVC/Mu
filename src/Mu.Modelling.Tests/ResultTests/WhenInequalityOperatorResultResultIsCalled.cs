namespace Mu.Modelling.ResultTests;

public sealed class WhenInequalityOperatorResultResultIsCalled
{
    [Test]
    public void GivenDifferentValuesThenReturnsTrue()
    {
        // Arrange
        Result left = ModellingTestData.CreateResult();
        Result right = ModellingTestData.CreateResult(name: ModellingTestData.CreateAlternateName());

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
        Result left = ModellingTestData.CreateResult();
        Result right = ModellingTestData.CreateResult();

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        resultLeftRight.ShouldBeFalse();
        resultRightLeft.ShouldBeFalse();
    }
}