namespace Mu.Modelling.FeatureTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public void GivenMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.Mutational;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(Feature.Kind.Mutational));
    }

    [Test]
    public void GivenNonMutationalValueThenReturnsName()
    {
        // Arrange
        Feature.Kind subject = Feature.Kind.NonMutational;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(Feature.Kind.NonMutational));
    }
}