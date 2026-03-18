namespace Mu.Modelling.UnitTests;

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
        Unit original = ModellingTestData.CreateUnit(attributes: ImmutableArray.Create(existing));

        // Act
        Unit result = original.AttributedWith(additional);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes.Concat([additional]));
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}