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
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Type).IsEqualTo(Feature.Kind.NonMutational);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}