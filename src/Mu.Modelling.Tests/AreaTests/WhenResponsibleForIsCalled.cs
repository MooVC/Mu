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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Units).IsEquivalentTo(original.Units.Concat([additional]));
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}