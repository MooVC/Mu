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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Default).IsEqualTo(updated);
        await Assert.That(result.Name).IsEqualTo(original.Name);
        await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}