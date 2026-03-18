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
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Source).IsEqualTo(NonMutational.Kind.WriteStore);
        _ = await Assert.That(result.View).IsEqualTo(original.View);
    }
}