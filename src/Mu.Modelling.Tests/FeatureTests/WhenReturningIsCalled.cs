namespace Mu.Modelling.FeatureTests;

using System.Collections.Immutable;

public sealed class WhenReturningIsCalled
{
    [Test]
    public async Task GivenResultThenReturnsUpdatedInstance()
    {
        // Arrange
        Result existing = ModellingTestData.CreateResult();
        Result additional = ModellingTestData.CreateResult(name: ModellingTestData.CreateAlternateName());
        Feature original = ModellingTestData.CreateFeature();

        // Act
        Feature result = original.Returning(existing).Returning(additional);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Results).IsEquivalentTo(ImmutableArray.Create(existing, additional));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}