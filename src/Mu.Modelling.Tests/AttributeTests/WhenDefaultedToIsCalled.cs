namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax.Elements;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenDefaultedToIsCalled
{
    private const string UpdatedDefaultValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        ModellingAttribute original = ModellingTestData.CreateAttribute();
        Snippet updated = Snippet.From(UpdatedDefaultValue);

        // Act
        ModellingAttribute result = original.DefaultedTo(updated);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Default).IsEqualTo(updated);
        await Assert.That(result.Name).IsEqualTo(original.Name);
        await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}