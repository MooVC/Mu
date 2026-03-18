namespace Mu.Modelling.ResultTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Result original = ModellingTestData.CreateResult();
        Name updated = UpdatedNameValue;

        // Act
        Result result = original.Named(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}