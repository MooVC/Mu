namespace Mu.Modelling.NonMutationalTests;

using MooVC.Syntax.Elements;

public sealed class WhenUsingIsCalled
{
    private const string UpdatedViewValue = "Updated";

    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();
        var updated = new View { Name = UpdatedViewValue };

        // Act
        NonMutational result = original.Using(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.View).IsEqualTo(updated);
        _ = await Assert.That(result.Source).IsEqualTo(original.Source);
    }
}