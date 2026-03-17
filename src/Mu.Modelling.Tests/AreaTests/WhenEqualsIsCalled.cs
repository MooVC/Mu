namespace Mu.Modelling.AreaTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Area left = ModellingTestData.CreateArea();
        Area right = ModellingTestData.CreateArea();

        // Act
        bool result = left.Equals(right);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNullThenReturnsFalse()
    {
        // Arrange
        Area subject = ModellingTestData.CreateArea();

        // Act
        bool result = subject.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }
}