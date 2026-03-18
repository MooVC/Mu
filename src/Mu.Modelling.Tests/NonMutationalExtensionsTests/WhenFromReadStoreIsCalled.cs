namespace Mu.Modelling.NonMutationalExtensionsTests;

public sealed class WhenFromReadStoreIsCalled
{
    [Test]
    public async Task GivenNonMutationalThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();

        // Act
        NonMutational result = original.FromReadStore();

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Source).IsEqualTo(NonMutational.Kind.ReadStore);
        _ = await Assert.That(result.View).IsEqualTo(original.View);
    }
}