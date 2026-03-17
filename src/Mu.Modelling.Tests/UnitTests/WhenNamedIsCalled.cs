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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Name).IsEqualTo(updated);
        await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes);
        await Assert.That(result.Features).IsEquivalentTo(original.Features);
        await Assert.That(result.Views).IsEquivalentTo(original.Views);
    }
}