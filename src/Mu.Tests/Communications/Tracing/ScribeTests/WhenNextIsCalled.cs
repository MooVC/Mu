namespace Mu.Communications.Tracing.ScribeTests;

using Mu.Modelling.Behavior;
using Mu.Testing;

public sealed class WhenNextIsCalled
{
    [Test]
    public async Task GivenFirstUseCaseThenUsesIdentityForCausationAndCorrelation()
    {
        // Arrange
        IScribe subject = new Scribe();
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);

        // Act
        using IDisposable scope = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(useCase.Identity);
    }

    [Test]
    public async Task GivenNestedUseCasesThenUsesParentIdentityAndPreservesCorrelation()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable root = subject.Next(first, out _);

        // Act
        using IDisposable child = subject.Next(second, out Ledger continuation);
        using IDisposable grandchild = subject.Next(third, out Ledger result);

        // Assert
        _ = await Assert.That(continuation.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(continuation.Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(result.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenNullUseCaseThenThrowsArgumentNullExceptionAndAllowsInitialization()
    {
        // Arrange
        const string expectedMessage = "The use case for the next ledger scope must be provided.";
        IScribe subject = new Scribe();
        UseCase useCase = null!;
        Ledger ledger = TestData.CreateLedger();
        var valid = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);

        // Act
        Exception? exception = Capture(() => _ = subject.Next(useCase, out _));
        using IDisposable scope = subject.Set(ledger);
        using IDisposable child = subject.Next(valid, out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(useCase));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
        _ = await Assert.That(result).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenNullUseCaseWhenScopeExistsThenPreservesLedgerAndParentIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        UseCase useCase = null!;
        using IDisposable root = subject.Next(first, out _);
        using IDisposable child = subject.Next(second, out _);

        // Act
        Exception? exception = Capture(() => _ = subject.Next(useCase, out _));
        using IDisposable grandchild = subject.Next(new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt), out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(useCase));
        _ = await Assert.That(result.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenSeededLedgerThenUsesSeedAndContinuesItsCorrelation()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        var first = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        using IDisposable incoming = subject.Set(ledger);

        // Act
        using IDisposable root = subject.Next(first, out Ledger initial);
        using IDisposable child = subject.Next(second, out Ledger result);

        // Assert
        _ = await Assert.That(initial).IsEqualTo(ledger);
        _ = await Assert.That(result.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(ledger.Correlation);
    }

    [Test]
    public async Task GivenSeparateScribesWhenContextIsSharedThenUsesActiveParent()
    {
        // Arrange
        IScribe subject = new Scribe();
        IScribe other = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable root = other.Next(first, out _);

        // Act
        using IDisposable child = subject.Next(second, out Ledger result);
        using IDisposable grandchild = other.Next(third, out Ledger shared);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(shared.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(shared.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenConcurrentChildrenThenIsolatesTheirLedgersAndPreservesParent()
    {
        // Arrange
        const int childCount = 2;
        const int timeoutSeconds = 10;
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int entered = 0;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using IDisposable root = subject.Next(first, out Ledger previous);

        // Act
        Ledger[] results = await Task.WhenAll(EnterChild(second), EnterChild(third));
        using IDisposable sibling = subject.Next(second, out Ledger unchanged);

        // Assert
        _ = await Assert.That(results[0].Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(results[1].Causation).IsEqualTo(third.Identity);
        _ = await Assert.That(results[0].Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(results[1].Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(unchanged).IsEqualTo(previous);

        async Task<Ledger> EnterChild(UseCase useCase)
        {
            using IDisposable child = subject.Next(useCase, out _);

            if (Interlocked.Increment(ref entered) == childCount)
            {
                ready.SetResult();
            }

            await ready.Task.WaitAsync(timeout.Token);

            using IDisposable grandchild = subject.Next(first, out Ledger result);

            return result;
        }
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