namespace Mu.Auditing.InMemoryAuditorTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenFailIsCalled
{
    [Test]
    public async Task GivenCapturedMessageThenStoresCauseAndPreservesOtherEntries()
    {
        // Arrange
        var cause = new InvalidOperationException(TestData.FailureMessage);
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        Guid otherIdentity = await subject.Capture(message, CancellationToken.None);

        // Act
        await subject.Fail(cause, identity, CancellationToken.None);

        // Assert
        InMemoryAuditor.Entry entry = subject.Entries.Single(candidate => candidate.Identity == identity);
        InMemoryAuditor.Entry other = subject.Entries.Single(candidate => candidate.Identity == otherIdentity);
        _ = await Assert.That(entry.Message).IsSameReferenceAs(message);
        _ = await Assert.That(entry.Outcome).IsNull();
        _ = await Assert.That(entry.Cause).IsSameReferenceAs(cause);
        _ = await Assert.That(other.Outcome).IsNull();
        _ = await Assert.That(other.Cause).IsNull();
    }

    [Test]
    public async Task GivenNullCauseThenThrowsArgumentNullExceptionAndPreservesEntry()
    {
        // Arrange
        Exception cause = null!;
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        InMemoryAuditor.Entry original = subject.Entries.Single();

        // Act
        Exception? exception = await Capture(() => subject.Fail(cause, identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(cause));
        _ = await Assert.That(subject.Entries.Single()).IsSameReferenceAs(original);
    }

    [Test]
    public async Task GivenEmptyIdentityThenThrowsArgumentException()
    {
        // Arrange
        Guid identity = Guid.Empty;
        var cause = new InvalidOperationException(TestData.FailureMessage);
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Fail(cause, identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
        _ = await Assert.That(((ArgumentException)exception!).ParamName).IsEqualTo(nameof(identity));
        _ = await Assert.That(subject.Entries.IsEmpty).IsTrue();
    }

    [Test]
    public async Task GivenUnknownIdentityThenThrowsInvalidOperationException()
    {
        // Arrange
        var cause = new InvalidOperationException(TestData.FailureMessage);
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Fail(cause, TestData.Identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(subject.Entries.IsEmpty).IsTrue();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenFinalizedEntryThenThrowsAndPreservesEntry(bool failed)
    {
        // Arrange
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var cause = new InvalidOperationException(TestData.FailureMessage);
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);

        if (failed)
        {
            await subject.Fail(cause, identity, CancellationToken.None);
        }
        else
        {
            await subject.Complete(identity, message.Yields<string>(TestData.ResultValue), CancellationToken.None);
        }

        InMemoryAuditor.Entry original = subject.Entries.Single();

        // Act
        Exception? exception = await Capture(() => subject.Fail(new InvalidOperationException(TestData.AlternateFailureMessage), identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(subject.Entries.Single()).IsSameReferenceAs(original);
    }

    [Test]
    public async Task GivenCanceledTokenThenThrowsAndPreservesEntry()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var cause = new InvalidOperationException(TestData.FailureMessage);
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        InMemoryAuditor.Entry original = subject.Entries.Single();
        await source.CancelAsync();

        // Act
        Exception? exception = await Capture(() => subject.Fail(cause, identity, source.Token));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<OperationCanceledException>();
        _ = await Assert.That(((OperationCanceledException)exception!).CancellationToken).IsEqualTo(source.Token);
        _ = await Assert.That(subject.Entries.Single()).IsSameReferenceAs(original);
    }

    private static async Task<Exception?> Capture(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}