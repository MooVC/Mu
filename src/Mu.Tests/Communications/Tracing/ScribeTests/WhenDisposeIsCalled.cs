namespace Mu.Communications.Tracing.ScribeTests;

using Mu.Testing;

public sealed class WhenDisposeIsCalled
{
    [Test]
    public async Task GivenNestedScopeThenRestoresLedgerAndParentIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable root = subject.Next(first);
        using IDisposable child = subject.Next(second);
        Ledger previous = subject.Ledger;
        using IDisposable grandchild = subject.Next(third);

        // Act
        grandchild.Dispose();
        Ledger restored = subject.Ledger;
        using IDisposable sibling = subject.Next(first);

        // Assert
        _ = await Assert.That(restored).IsEqualTo(previous);
        _ = await Assert.That(subject.Ledger.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(subject.Ledger.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenCompletedChildThenSiblingUsesRootIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable root = subject.Next(first);
        using IDisposable child = subject.Next(second);

        // Act
        child.Dispose();
        using IDisposable sibling = subject.Next(third);

        // Assert
        _ = await Assert.That(subject.Ledger.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(subject.Ledger.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenRootScopeThenClearsLedgerAndAllowsIndependentRoot()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        using IDisposable root = subject.Next(first);

        // Act
        root.Dispose();
        Exception? exception = Capture(() => _ = subject.Ledger);
        using IDisposable replacement = subject.Next(second);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(subject.Ledger.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(subject.Ledger.Correlation).IsEqualTo(second.Identity);
    }

    [Test]
    public async Task GivenSeededScopeThenClearsLedgerAndAllowsReplacement()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        using IDisposable incoming = subject.Set(ledger);

        // Act
        incoming.Dispose();
        Exception? exception = Capture(() => _ = subject.Ledger);
        using IDisposable scope = subject.Set(replacement);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(subject.Ledger).IsEqualTo(replacement);
    }

    [Test]
    public async Task GivenSeededChildThenRestoresIncomingLedgerWithoutUseCaseIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        var first = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable incoming = subject.Set(ledger);
        using IDisposable child = subject.Next(first);

        // Act
        child.Dispose();
        Ledger restored = subject.Ledger;
        using IDisposable sibling = subject.Next(second);

        // Assert
        _ = await Assert.That(restored).IsEqualTo(ledger);
        _ = await Assert.That(subject.Ledger).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenRepeatedDisposalThenPreservesActiveScope()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        using IDisposable original = subject.Set(ledger);
        original.Dispose();
        using IDisposable current = subject.Set(replacement);

        // Act
        original.Dispose();

        // Assert
        _ = await Assert.That(subject.Ledger).IsEqualTo(replacement);
    }

    [Test]
    public async Task GivenActiveChildThenThrowsAndAllowsOrderedCleanup()
    {
        // Arrange
        const string expectedMessage = "Nested ledger scopes must be disposed before their parent scope.";
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        using IDisposable root = subject.Next(first);
        using IDisposable child = subject.Next(second);
        Ledger previous = subject.Ledger;

        // Act
        Exception? exception = Capture(root.Dispose);
        Ledger unchanged = subject.Ledger;
        child.Dispose();
        root.Dispose();
        using IDisposable replacement = subject.Set(ledger);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(unchanged).IsEqualTo(previous);
        _ = await Assert.That(subject.Ledger).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenExceptionInChildThenRestoresParent()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        var failure = new InvalidOperationException(TestData.FailureMessage);
        using IDisposable root = subject.Next(first);
        Ledger previous = subject.Ledger;

        // Act
        Exception? exception = Capture(() =>
        {
            using IDisposable child = subject.Next(second);
            throw failure;
        });
        Ledger restored = subject.Ledger;
        using IDisposable sibling = subject.Next(third);

        // Assert
        _ = await Assert.That(exception).IsSameReferenceAs(failure);
        _ = await Assert.That(restored).IsEqualTo(previous);
        _ = await Assert.That(subject.Ledger.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(subject.Ledger.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenInheritedScopeWhenDisposedInChildContextThenParentCanStillDispose()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        using IDisposable root = subject.Set(ledger);

        // Act
        await Task.Run(root.Dispose);
        Ledger unchanged = subject.Ledger;
        root.Dispose();
        using IDisposable current = subject.Set(replacement);

        // Assert
        _ = await Assert.That(unchanged).IsEqualTo(ledger);
        _ = await Assert.That(subject.Ledger).IsEqualTo(replacement);
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