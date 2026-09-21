namespace Mu.Modelling.IdentityTests;

using MooVC.Syntax;

public sealed class WhenGetSymbolIsCalled
{
    [Test]
    public async Task GivenNullNamespaceThenThrowsWithResourceMessage()
    {
        // Arrange
        const string expectedMessage = "The namespace must be provided.";
        Qualifier @namespace = null!;
        Exception? exception = null;

        // Act
        try
        {
            _ = Identity.Default.GetSymbol(@namespace);
        }
        catch (ArgumentNullException failure)
        {
            exception = failure;
        }

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(exception!.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }
}