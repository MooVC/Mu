namespace Mu.Modelling.FeatureTests;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Feature original = ModellingTestData.CreateFeature();

        // Act
        Feature result = original.OfType(Feature.Kind.NonMutational);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(Feature.Kind.NonMutational);
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}