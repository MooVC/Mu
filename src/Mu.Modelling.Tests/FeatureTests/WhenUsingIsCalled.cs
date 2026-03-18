namespace Mu.Modelling.FeatureTests;

using System.Linq;

public sealed class WhenUsingIsCalled
{
    [Test]
    public async Task GivenParameterThenReturnsUpdatedInstance()
    {
        // Arrange
        Parameter existing = ModellingTestData.CreateParameter();
        Parameter additional = ModellingTestData.CreateParameter(name: ModellingTestData.CreateAlternateName());
        Feature original = ModellingTestData.CreateFeature(parameters: existing);

        // Act
        Feature result = original.Using(additional);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Parameters).IsEquivalentTo(original.Parameters.Concat([additional]));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}