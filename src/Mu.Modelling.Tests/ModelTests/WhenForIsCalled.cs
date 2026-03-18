namespace Mu.Modelling.ModelTests;

using MooVC.Syntax.Elements;

public sealed class WhenForIsCalled
{
    private const string UpdatedCompanyValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Model original = ModellingTestData.CreateModel();
        var updated = new Name(UpdatedCompanyValue);

        // Act
        Model result = original.For(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Company).IsEqualTo(updated);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
        _ = await Assert.That(result.Areas).IsEquivalentTo(original.Areas);
    }
}