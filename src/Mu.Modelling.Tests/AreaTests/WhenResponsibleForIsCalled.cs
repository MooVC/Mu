namespace Mu.Modelling.AreaTests;

using System.Linq;

public sealed class WhenResponsibleForIsCalled
{
    [Test]
    public async Task GivenUnitThenReturnsUpdatedInstance()
    {
        // Arrange
        Unit existing = ModellingTestData.CreateUnit();
        Unit additional = ModellingTestData.CreateUnit(ModellingTestData.CreateAlternateName());
        Area original = ModellingTestData.CreateArea(units: existing);

        // Act
        Area result = original.ResponsibleFor(additional);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Units).IsEquivalentTo(original.Units.Concat([additional]));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}