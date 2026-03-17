namespace Mu.Modelling.UnitTests;

using System.Collections.Immutable;
using System.Linq;

public sealed class WhenSeenAsIsCalled
{
    [Test]
    public async Task GivenViewThenReturnsUpdatedInstance()
    {
        // Arrange
        View existing = ModellingTestData.CreateView();
        View additional = ModellingTestData.CreateView(name: ModellingTestData.CreateAlternateName());
        Unit original = ModellingTestData.CreateUnit(views: ImmutableArray.Create(existing));

        // Act
        Unit result = original.SeenAs(additional);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Views).IsEquivalentTo(original.Views.Concat([additional]));
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}