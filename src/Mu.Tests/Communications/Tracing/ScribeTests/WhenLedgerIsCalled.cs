namespace Mu.Communications.Tracing.ScribeTests;

public sealed class WhenLedgerIsCalled
{
    [Test]
    public async Task GivenNoActiveScopeThenThrowsInvalidOperationException()
    {
        // Arrange
        const string expectedMessage = "A ledger scope must be established before the current ledger can be read.";
        IScribe subject = new Scribe();
        Exception? result = default;

        // Act
        try
        {
            _ = subject.Ledger;
        }
        catch (Exception exception)
        {
            result = exception;
        }

        // Assert
        _ = await Assert.That(result).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(result!.Message).IsEqualTo(expectedMessage);
    }
}