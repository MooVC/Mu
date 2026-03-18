namespace Mu.Modelling.UnitTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Unit original = ModellingTestData.CreateUnit();
        Name updated = UpdatedNameValue;

        // Act
        Unit result = original.Named(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes);
        _ = await Assert.That(result.Features).IsEquivalentTo(original.Features);
        _ = await Assert.That(result.Views).IsEquivalentTo(original.Views);
    }
}