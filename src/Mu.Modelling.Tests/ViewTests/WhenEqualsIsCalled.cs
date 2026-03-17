namespace Mu.Modelling.ViewTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        View left = ModellingTestData.CreateView();
        View right = ModellingTestData.CreateView();

        // Act
        bool result = left.Equals(right);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNullThenReturnsFalse()
    {
        // Arrange
        View subject = ModellingTestData.CreateView();

        // Act
        bool result = subject.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }
}