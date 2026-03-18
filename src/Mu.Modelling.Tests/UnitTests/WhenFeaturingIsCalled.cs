namespace Mu.Modelling.UnitTests;

using System.Collections.Immutable;
using System.Linq;

public sealed class WhenFeaturingIsCalled
{
    [Test]
    public async Task GivenFeatureThenReturnsUpdatedInstance()
    {
        // Arrange
        Feature existing = ModellingTestData.CreateFeature();
        Feature additional = ModellingTestData.CreateFeature(name: ModellingTestData.CreateAlternateName());
        Unit original = ModellingTestData.CreateUnit(features: ImmutableArray.Create(existing));

        // Act
        Unit result = original.Featuring(additional);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Features).IsEquivalentTo(original.Features.Concat([additional]));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}