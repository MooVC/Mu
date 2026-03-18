namespace Mu.Modelling.ModelTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Model left = ModellingTestData.CreateModel();
        Model right = ModellingTestData.CreateModel();

        // Act
        bool result = left.Equals(right);

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        Model subject = ModellingTestData.CreateModel();

        // Act
        bool result = subject.Equals(null);

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}