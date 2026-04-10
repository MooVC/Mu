namespace Mu.Modelling.FeatureTests;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Feature original = ModellingTestData.CreateFeature();

        // Act
        Feature result = original.OfType(Feature.Kinds.NonMutational);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Type).IsEqualTo(Feature.Kinds.NonMutational);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}