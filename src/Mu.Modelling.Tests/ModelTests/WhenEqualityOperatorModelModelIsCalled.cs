namespace Mu.Modelling.ModelTests;

public sealed class WhenEqualityOperatorModelModelIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Model left = ModellingTestData.CreateModel();
        Model right = ModellingTestData.CreateModel();

        // Act
        bool resultLeftRight = left == right;
        bool resultRightLeft = right == left;

        // Assert
        await Assert.That(resultLeftRight).IsTrue();
        await Assert.That(resultRightLeft).IsTrue();
    }

    [Test]
    public async Task GivenDifferentValuesThenReturnsFalse()
    {
        // Arrange
        Model left = ModellingTestData.CreateModel();
        Model right = ModellingTestData.CreateModel(name: ModellingTestData.CreateAlternateName());

        // Act
        bool resultLeftRight = left == right;
        bool resultRightLeft = right == left;

        // Assert
        await Assert.That(resultLeftRight).IsFalse();
        await Assert.That(resultRightLeft).IsFalse();
    }
}