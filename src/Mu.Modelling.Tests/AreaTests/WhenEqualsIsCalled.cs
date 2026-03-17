namespace Mu.Modelling.AreaTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Area left = ModellingTestData.CreateArea();
        Area right = ModellingTestData.CreateArea();

        // Act
        bool result = left.Equals(right);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        Area subject = ModellingTestData.CreateArea();

        // Act
        bool result = subject.Equals(null);

        // Assert
        await Assert.That(result).IsFalse();
    }
}