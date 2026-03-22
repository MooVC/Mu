namespace Mu.Modelling.AreaTests;

using MooVC.Syntax;

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
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Units).IsEquivalentTo(original.Units);
    }
}