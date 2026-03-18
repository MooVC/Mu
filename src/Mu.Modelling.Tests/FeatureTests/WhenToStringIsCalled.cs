namespace Mu.Modelling.FeatureTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string FeatureNameValue = "FeatureName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(FeatureNameValue);
        Feature subject = ModellingTestData.CreateFeature(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result).Contains(nameof(Feature));
        await Assert.That(result).Contains(FeatureNameValue);
    }
}