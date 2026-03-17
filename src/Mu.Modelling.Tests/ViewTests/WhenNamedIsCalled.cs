namespace Mu.Modelling.ViewTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        View original = ModellingTestData.CreateView();
        Name updated = UpdatedNameValue;

        // Act
        View result = original.Named(updated);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Name).IsEqualTo(updated);
        await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes);
        await Assert.That(result.Facts).IsEqualTo(original.Facts);
    }
}