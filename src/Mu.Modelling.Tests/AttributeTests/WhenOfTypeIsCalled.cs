namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax.CSharp;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        ModellingAttribute original = ModellingTestData.CreateAttribute();
        Symbol updated = ModellingTestData.CreateSymbol(typeof(Guid));

        // Act
        ModellingAttribute result = original.OfType(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Type).IsEqualTo(updated);
        _ = await Assert.That(result.Default).IsEqualTo(original.Default);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}