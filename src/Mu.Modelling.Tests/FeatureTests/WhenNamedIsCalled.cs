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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Name).IsEqualTo(updated);
        await Assert.That(result.Parameters).IsEquivalentTo(original.Parameters);
        await Assert.That(result.Results).IsEquivalentTo(original.Results);
        await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}