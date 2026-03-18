namespace Mu.Modelling.ParameterTests;

using MooVC.Syntax.Elements;

public sealed class WhenNamedIsCalled
{
    private const string UpdatedNameValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Parameter original = ModellingTestData.CreateParameter();
        Name updated = UpdatedNameValue;

        // Act
        Parameter result = original.Named(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Name).IsEqualTo(updated);
        _ = await Assert.That(result.Default).IsEqualTo(original.Default);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}