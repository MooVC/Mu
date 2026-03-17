namespace Mu.Modelling.ViewTests;

using System.Collections.Immutable;
using System.Linq;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenAttributedWithIsCalled
{
    [Test]
    public async Task GivenAttributeThenReturnsUpdatedInstance()
    {
        // Arrange
        ModellingAttribute existing = ModellingTestData.CreateAttribute();
        ModellingAttribute additional = ModellingTestData.CreateAttribute(name: ModellingTestData.CreateAlternateName());
        View original = ModellingTestData.CreateView(attributes: ImmutableArray.Create(existing));

        // Act
        View result = original.AttributedWith(additional);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes.Concat([additional]));
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}