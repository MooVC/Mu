namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax.CSharp.Elements;
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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(updated);
        await Assert.That(result.Default).IsEqualTo(original.Default);
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}