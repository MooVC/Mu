namespace Mu.Communications.Tracing.ScribeTests;

using Mu.Testing;

public sealed class WhenSetIsCalled
{
    [Test]
    public async Task GivenDefaultLedgerThenAcceptsLedgerAndRejectsReplacement()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = default;

        // Act
        using IDisposable scope = subject.Set(ledger);
        Exception? exception = Capture(() => _ = subject.Set(TestData.CreateLedger()));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(subject.Ledger).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenExistingLedgerThenThrowsInvalidOperationExceptionAndPreservesLedger()
    {
        // Arrange
        const string expectedMessage = "A ledger scope is already active in the current execution context.";
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        using IDisposable scope = subject.Set(ledger);

        // Act
        Exception? exception = Capture(() => _ = subject.Set(replacement));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(subject.Ledger).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenExistingUseCaseThenThrowsInvalidOperationExceptionAndPreservesParentIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        using IDisposable root = subject.Next(first);
        using IDisposable child = subject.Next(second);
        Ledger previous = subject.Ledger;

        // Act
        Exception? exception = Capture(() => _ = subject.Set(TestData.CreateLedger()));
        Ledger unchanged = subject.Ledger;
        using IDisposable grandchild = subject.Next(new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(unchanged).IsEqualTo(previous);
        _ = await Assert.That(subject.Ledger.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(subject.Ledger.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenNoCurrentLedgerThenEstablishesLedgerImmediately()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();

        // Act
        using IDisposable scope = subject.Set(ledger);

        // Assert
        _ = await Assert.That(subject.Ledger).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenSameLedgerThenThrowsInvalidOperationException()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        using IDisposable scope = subject.Set(ledger);

        // Act
        Exception? exception = Capture(() => _ = subject.Set(ledger));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}