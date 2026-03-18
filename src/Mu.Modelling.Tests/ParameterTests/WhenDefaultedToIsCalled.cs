namespace Mu.Modelling.ParameterTests;

using MooVC.Syntax.Elements;

public sealed class WhenDefaultedToIsCalled
{
    private const string UpdatedDefaultValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Parameter original = ModellingTestData.CreateParameter();
        Snippet updated = Snippet.From(UpdatedDefaultValue);

        // Act
        Parameter result = original.DefaultedTo(updated);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Default).IsEqualTo(updated);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
        _ = await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}