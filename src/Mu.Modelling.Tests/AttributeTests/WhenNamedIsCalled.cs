namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax.Elements;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        ModellingAttribute original = ModellingTestData.CreateAttribute();
        Name updated = UpdatedNameValue;

        // Act
        ModellingAttribute result = original.Named(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Default).IsEqualTo(original.Default);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}