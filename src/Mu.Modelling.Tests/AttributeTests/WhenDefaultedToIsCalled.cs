namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenDefaultedToIsCalled
{
    private const string UpdatedDefaultValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        ModellingAttribute original = ModellingTestData.CreateAttribute();
        var updated = Snippet.From(UpdatedDefaultValue);

        // Act
        ModellingAttribute result = original.DefaultedTo(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Default).IsEqualTo(updated);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}