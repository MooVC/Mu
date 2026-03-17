namespace Mu.Modelling.AreaTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Area original = ModellingTestData.CreateArea();
        Name updated = UpdatedNameValue;

        // Act
        Area result = original.Named(updated);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Name).IsEqualTo(updated);
        await Assert.That(result.Units).IsEquivalentTo(original.Units);
    }
}