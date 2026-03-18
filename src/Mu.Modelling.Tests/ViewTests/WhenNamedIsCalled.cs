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
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Attributes).IsEquivalentTo(original.Attributes);
        _ = await Assert.That(result.Facts).IsEqualTo(original.Facts);
    }
}