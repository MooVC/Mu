namespace Mu.Modelling.ViewTests;

using System.Collections.Immutable;
using System.Linq;
using MooVC.Syntax.Elements;

public sealed class WhenRenderedOnIsCalled
{
    private const string SecondaryQualifierValue = "Mu.Modelling.Secondary";

    [Test]
    public async Task GivenQualifierThenReturnsUpdatedInstance()
    {
        // Arrange
        Qualifier existing = ModellingTestData.CreateQualifier();
        Qualifier additional = ModellingTestData.CreateQualifier(SecondaryQualifierValue);
        View original = ModellingTestData.CreateView(facts: ImmutableArray.Create(existing));

        // Act
        View result = original.RenderedOn(additional);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Facts).IsEquivalentTo(original.Facts.Concat([additional]));
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}