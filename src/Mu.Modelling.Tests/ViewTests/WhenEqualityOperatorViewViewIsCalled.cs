namespace Mu.Modelling.ViewTests;

public sealed class WhenEqualityOperatorViewViewIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        View left = ModellingTestData.CreateView();
        View right = ModellingTestData.CreateView();

        // Act
        bool resultLeftRight = left == right;
        bool resultRightLeft = right == left;

        // Assert
        resultLeftRight.ShouldBeTrue();
        resultRightLeft.ShouldBeTrue();
    }

    [Test]
    public void GivenDifferentValuesThenReturnsFalse()
    {
        // Arrange
        View left = ModellingTestData.CreateView();
        View right = ModellingTestData.CreateView(name: ModellingTestData.CreateAlternateName());

        // Act
        bool resultLeftRight = left == right;
        bool resultRightLeft = right == left;

        // Assert
        resultLeftRight.ShouldBeFalse();
        resultRightLeft.ShouldBeFalse();
    }
}