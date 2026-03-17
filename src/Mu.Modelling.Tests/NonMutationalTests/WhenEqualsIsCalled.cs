namespace Mu.Modelling.NonMutationalTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        NonMutational left = ModellingTestData.CreateNonMutational();
        NonMutational right = ModellingTestData.CreateNonMutational();

        // Act
        bool result = left.Equals(right);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNullThenReturnsFalse()
    {
        // Arrange
        NonMutational subject = ModellingTestData.CreateNonMutational();

        // Act
        bool result = subject.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }
}