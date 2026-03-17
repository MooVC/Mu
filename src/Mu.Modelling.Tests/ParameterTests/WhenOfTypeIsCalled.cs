namespace Mu.Modelling.ParameterTests;

using Symbol = MooVC.Syntax.CSharp.Elements.Symbol;

public sealed class WhenOfTypeIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsUpdatedInstance()
    {
        // Arrange
        Parameter original = ModellingTestData.CreateParameter();
        Symbol updated = ModellingTestData.CreateSymbol(typeof(Guid));

        // Act
        Parameter result = original.OfType(updated);

        // Assert
        await Assert.That(!ReferenceEquals(result, original)).IsTrue();
        await Assert.That(result.Type).IsEqualTo(updated);
        await Assert.That(result.Default).IsEqualTo(original.Default);
        await Assert.That(result.Name).IsEqualTo(original.Name);
    }
}