namespace Mu.Modelling.ResultTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Result left = ModellingTestData.CreateResult();
        Result right = ModellingTestData.CreateResult();

        // Act
        bool result = left.Equals(right);

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        Result subject = ModellingTestData.CreateResult();

        // Act
        bool result = subject.Equals(null);

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}