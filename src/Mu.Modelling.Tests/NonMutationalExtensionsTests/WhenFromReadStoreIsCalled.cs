namespace Mu.Modelling.NonMutationalExtensionsTests;

public sealed class WhenFromReadStoreIsCalled
{
    [Test]
    public void GivenNonMutationalThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();

        // Act
        NonMutational result = original.FromReadStore();

        // Assert
        result.ShouldNotBeSameAs(original);
        result.Source.ShouldBe(NonMutational.Kind.ReadStore);
        result.View.ShouldBe(original.View);
    }
}