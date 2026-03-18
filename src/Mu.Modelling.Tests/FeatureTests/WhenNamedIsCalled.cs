namespace Mu.Modelling.FeatureTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Feature original = ModellingTestData.CreateFeature();
        Name updated = UpdatedNameValue;

        // Act
        Feature result = original.Named(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Parameters).IsEquivalentTo(original.Parameters);
        _ = await Assert.That(result.Results).IsEquivalentTo(original.Results);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}