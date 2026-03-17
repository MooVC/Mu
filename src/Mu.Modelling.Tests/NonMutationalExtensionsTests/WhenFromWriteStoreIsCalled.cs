namespace Mu.Modelling.NonMutationalExtensionsTests;

public sealed class WhenFromWriteStoreIsCalled
{
    [Test]
    public async Task GivenNonMutationalThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();

        // Act
        NonMutational result = original.FromWriteStore();

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Source).IsEqualTo(NonMutational.Kind.WriteStore);
        await Assert.That(result.View).IsEqualTo(original.View);
    }
}