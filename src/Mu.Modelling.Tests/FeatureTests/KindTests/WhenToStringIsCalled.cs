namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Feature.Kind.Mutational));
    }

    [Test]
    public async Task GivenNonMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Feature.Kind.NonMutational));
    }
}