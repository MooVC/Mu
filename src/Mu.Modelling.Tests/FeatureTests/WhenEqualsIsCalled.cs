namespace Mu.Modelling.FeatureTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        Feature left = ModellingTestData.CreateFeature();
        Feature right = ModellingTestData.CreateFeature();

        // Act
        bool result = left.Equals(right);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenNullThenReturnsFalse()
    {
        // Arrange
        Feature subject = ModellingTestData.CreateFeature();

        // Act
        bool result = subject.Equals(null);

        // Assert
        await Assert.That(result).IsFalse();
    }
}