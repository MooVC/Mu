namespace Mu.Modelling.NonMutationalTests;

public sealed class WhenFromIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();

        // Act
        NonMutational result = original.From(NonMutational.Kinds.WriteStore);

        // Assert
        _ = await Assert.That(result).IsNotSameReferenceAs(original);
        _ = await Assert.That(result.Source).IsEqualTo(NonMutational.Kinds.WriteStore);
        _ = await Assert.That(result.View).IsEqualTo(original.View);
    }
}