namespace Mu.Modelling.ModelTests;

using System.Linq;

public sealed class WhenWithAreaIsCalled
{
    [Test]
    public async Task GivenAreaThenReturnsUpdatedInstance()
    {
        // Arrange
        Area existing = ModellingTestData.CreateArea();
        Area additional = ModellingTestData.CreateArea(name: ModellingTestData.CreateAlternateName());
        Model original = ModellingTestData.CreateModel(areas: existing);

        // Act
        Model result = original.WithArea(additional);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Areas).IsEquivalentTo(original.Areas.Concat([additional]));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}