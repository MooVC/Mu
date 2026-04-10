namespace Mu.Modelling.FeatureTests.KindsTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.Mutational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Feature.Kinds.Mutational));
    }

    [Test]
    public async Task GivenNonMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kinds subject = Feature.Kinds.NonMutational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Feature.Kinds.NonMutational));
    }
}