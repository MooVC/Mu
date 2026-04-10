namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;
        Feature.Kinds other = Feature.Kinds.NonMutational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenSameValueThenReturnsTrue()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;
        Feature.Kinds other = Feature.Kinds.Mutational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}