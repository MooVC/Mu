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
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Source).IsEqualTo(NonMutational.Kind.ReadStore);
        await Assert.That(result.View).IsEqualTo(original.View);
    }
}