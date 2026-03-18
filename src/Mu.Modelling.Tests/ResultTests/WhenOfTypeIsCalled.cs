namespace Mu.Modelling.ResultTests;

using Symbol = MooVC.Syntax.CSharp.Elements.Symbol;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Result original = ModellingTestData.CreateResult();
        Symbol updated = ModellingTestData.CreateSymbol(typeof(Guid));

        // Act
        Result result = original.OfType(updated);

        // Assert
        _ = await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        _ = await Assert.That(result.Type).IsEqualTo(updated);
        _ = await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}