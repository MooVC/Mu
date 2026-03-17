namespace Mu.Modelling.MutationalTests;

using MooVC.Syntax.Elements;

public sealed class WhenRaisesIsCalled
{
    private const string UpdatedFactValue = "Updated";

    [Test]
    public void GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();
        Name updated = UpdatedFactValue;

        // Act
        Mutational result = original.Raises(updated);

        // Assert
        result.ShouldNotBeSameAs(original);
        result.Fact.ShouldBe(updated);
        result.Type.ShouldBe(original.Type);
    }
}