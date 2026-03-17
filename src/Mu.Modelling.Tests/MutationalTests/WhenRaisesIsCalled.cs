namespace Mu.Modelling.MutationalTests;

using MooVC.Syntax.Elements;

public sealed class WhenRaisesIsCalled
{
    private const string UpdatedFactValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();
        Name updated = UpdatedFactValue;

        // Act
        Mutational result = original.Raises(updated);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Fact).IsEqualTo(updated);
        await Assert.That(result.Type).IsEqualTo(original.Type);
    }
}