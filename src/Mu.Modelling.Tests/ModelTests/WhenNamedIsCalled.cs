namespace Mu.Modelling.ModelTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Model original = ModellingTestData.CreateModel();
        Name updated = UpdatedNameValue;

        // Act
        Model result = original.Named(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Company).IsEqualTo(original.Company);
        _ = await Assert.That(result.Areas).IsEquivalentTo(original.Areas);
    }
}