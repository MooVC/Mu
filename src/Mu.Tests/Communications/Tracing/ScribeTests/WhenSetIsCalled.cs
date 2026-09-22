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
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);

        // Act
        using IDisposable scope = subject.Set(ledger);
        Exception? exception = Capture(() => _ = subject.Set(TestData.CreateLedger()));
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(result).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenExistingLedgerThenThrowsInvalidOperationExceptionAndPreservesLedger()
    {
        // Arrange
        const string expectedMessage = "A ledger scope is already active in the current execution context.";
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        using IDisposable scope = subject.Set(ledger);

        // Act
        Exception? exception = Capture(() => _ = subject.Set(replacement));
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(result).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenExistingUseCaseThenThrowsInvalidOperationExceptionAndPreservesParentIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        using IDisposable root = subject.Next(first, out _);
        using IDisposable child = subject.Next(second, out _);

        // Act
        Exception? exception = Capture(() => _ = subject.Set(TestData.CreateLedger()));
        using IDisposable grandchild = subject.Next(new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt), out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(result.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenNoCurrentLedgerThenSeedsNextUseCase()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);

        // Act
        using IDisposable scope = subject.Set(ledger);
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(result).IsEqualTo(ledger);
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