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
        using IDisposable root = subject.Next(first, out _);
        using IDisposable child = subject.Next(second, out _);
        using IDisposable grandchild = subject.Next(third, out Ledger previous);

        // Act
        grandchild.Dispose();
        using IDisposable sibling = subject.Next(first, out Ledger restored);

        // Assert
        _ = await Assert.That(restored).IsEqualTo(previous);
        _ = await Assert.That(restored.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(restored.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenCompletedChildThenSiblingUsesRootIdentity()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var third = new TestData.TestMutation(TestData.Correlation, TestData.ProposedAt);
        using IDisposable root = subject.Next(first, out _);
        using IDisposable child = subject.Next(second, out _);

        // Act
        child.Dispose();
        using IDisposable sibling = subject.Next(third, out Ledger result);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenRootScopeThenClearsLedgerAndAllowsIndependentRoot()
    {
        // Arrange
        IScribe subject = new Scribe();
        var first = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        var second = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        using IDisposable root = subject.Next(first, out _);

        // Act
        root.Dispose();
        using IDisposable replacement = subject.Next(second, out Ledger result);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(second.Identity);
    }

    [Test]
    public async Task GivenSeededScopeThenClearsLedgerAndAllowsReplacement()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        using IDisposable incoming = subject.Set(ledger);

        // Act
        incoming.Dispose();
        using IDisposable scope = subject.Set(replacement);
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(result).IsEqualTo(replacement);
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
        using IDisposable child = subject.Next(first, out _);

        // Act
        child.Dispose();
        using IDisposable sibling = subject.Next(second, out Ledger restored);

        // Assert
        _ = await Assert.That(restored).IsEqualTo(ledger);
    }

    [Test]
    public async Task GivenRepeatedDisposalThenPreservesActiveScope()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        using IDisposable original = subject.Set(ledger);
        original.Dispose();
        using IDisposable current = subject.Set(replacement);

        // Act
        original.Dispose();
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(result).IsEqualTo(replacement);
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
        using IDisposable root = subject.Next(first, out _);
        using IDisposable child = subject.Next(second, out _);

        // Act
        Exception? exception = Capture(root.Dispose);
        using IDisposable probe = subject.Next(first, out Ledger unchanged);
        probe.Dispose();
        child.Dispose();
        root.Dispose();
        using IDisposable replacement = subject.Set(ledger);
        using IDisposable initialized = subject.Next(first, out Ledger result);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(unchanged.Causation).IsEqualTo(second.Identity);
        _ = await Assert.That(unchanged.Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(result).IsEqualTo(ledger);
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
        using IDisposable root = subject.Next(first, out Ledger previous);

        // Act
        Exception? exception = Capture(() =>
        {
            using IDisposable child = subject.Next(second, out _);
            throw failure;
        });
        using IDisposable sibling = subject.Next(third, out Ledger restored);

        // Assert
        _ = await Assert.That(exception).IsSameReferenceAs(failure);
        _ = await Assert.That(restored).IsEqualTo(previous);
        _ = await Assert.That(restored.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(restored.Correlation).IsEqualTo(first.Identity);
    }

    [Test]
    public async Task GivenInheritedScopeWhenDisposedInChildContextThenParentCanStillDispose()
    {
        // Arrange
        IScribe subject = new Scribe();
        Ledger ledger = TestData.CreateLedger();
        Ledger replacement = TestData.CreateLedger(TestData.AlternateIdentity, TestData.Identity);
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);
        using IDisposable root = subject.Set(ledger);

        // Act
        await Task.Run(root.Dispose);
        using IDisposable probe = subject.Next(useCase, out Ledger unchanged);
        probe.Dispose();
        root.Dispose();
        using IDisposable current = subject.Set(replacement);
        using IDisposable child = subject.Next(useCase, out Ledger result);

        // Assert
        _ = await Assert.That(unchanged).IsEqualTo(ledger);
        _ = await Assert.That(result).IsEqualTo(replacement);
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