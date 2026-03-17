namespace Mu.Modelling.NonMutationalTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        NonMutational left = ModellingTestData.CreateNonMutational();
        NonMutational right = ModellingTestData.CreateNonMutational();

        // Act
        bool result = left.Equals(right);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        NonMutational subject = ModellingTestData.CreateNonMutational();

        // Act
        bool result = subject.Equals(null);

        // Assert
        await Assert.That(result).IsFalse();
    }
}