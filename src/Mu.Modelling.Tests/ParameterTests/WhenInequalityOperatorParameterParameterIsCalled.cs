namespace Mu.Modelling.ParameterTests;

public sealed class WhenInequalityOperatorParameterParameterIsCalled
{
    [Test]
    public async Task GivenDifferentValuesThenReturnsTrue()
    {
        // Arrange
        Parameter left = ModellingTestData.CreateParameter();
        Parameter right = ModellingTestData.CreateParameter(name: ModellingTestData.CreateAlternateName());

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        _ = await Assert.That(resultLeftRight).IsTrue();
        _ = await Assert.That(resultRightLeft).IsTrue();
    }

    [Test]
    public async Task GivenEqualValuesThenReturnsFalse()
    {
        // Arrange
        Parameter left = ModellingTestData.CreateParameter();
        Parameter right = ModellingTestData.CreateParameter();

        // Act
        bool resultLeftRight = left != right;
        bool resultRightLeft = right != left;

        // Assert
        _ = await Assert.That(resultLeftRight).IsFalse();
        _ = await Assert.That(resultRightLeft).IsFalse();
    }
}