namespace Mu.Modelling.NonMutationalTests;

public sealed class WhenFromIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        NonMutational original = ModellingTestData.CreateNonMutational();

        // Act
        NonMutational result = original.From(NonMutational.Kind.WriteStore);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Source).IsEqualTo(NonMutational.Kind.WriteStore);
        await Assert.That(result.View).IsEqualTo(original.View);
    }
}