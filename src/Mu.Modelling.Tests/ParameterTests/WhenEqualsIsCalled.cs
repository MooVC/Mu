namespace Mu.Modelling.ParameterTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Parameter left = ModellingTestData.CreateParameter();
        Parameter right = ModellingTestData.CreateParameter();

        // Act
        bool result = left.Equals(right);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNullThenReturnsFalse()
    {
        // Arrange
        Parameter subject = ModellingTestData.CreateParameter();

        // Act
        bool result = subject.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }
}