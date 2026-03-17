namespace Mu.Modelling.ParameterTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Parameter left = ModellingTestData.CreateParameter();
        Parameter right = ModellingTestData.CreateParameter();

        // Act
        bool result = left.Equals(right);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        Parameter subject = ModellingTestData.CreateParameter();

        // Act
        bool result = subject.Equals(null);

        // Assert
        await Assert.That(result).IsFalse();
    }
}