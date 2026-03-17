namespace Mu.Modelling.MutationalTests;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public void GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Mutational original = ModellingTestData.CreateMutational();

        // Act
        Mutational result = original.OfType(Mutational.Kind.Transitional);

        // Assert
        result.ShouldNotBeSameAs(original);
        result.Type.ShouldBe(Mutational.Kind.Transitional);
        result.Fact.ShouldBe(original.Fact);
    }
}