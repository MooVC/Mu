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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Parameters).IsEquivalentTo(original.Parameters.Concat([additional]));
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}