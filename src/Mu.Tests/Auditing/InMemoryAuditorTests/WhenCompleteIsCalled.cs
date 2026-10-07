namespace Mu.Auditing.InMemoryAuditorTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenCompleteIsCalled
{
    [Test]
    public async Task GivenCapturedMessageThenStoresOutcomeAndPreservesOtherEntries()
    {
        // Arrange
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        Guid otherIdentity = await subject.Capture(message, CancellationToken.None);

        // Act
        await subject.Complete(identity, outcome, CancellationToken.None);

        // Assert
        InMemoryAuditor.Entry entry = subject.Entries.Single(candidate => candidate.Identity == identity);
        InMemoryAuditor.Entry other = subject.Entries.Single(candidate => candidate.Identity == otherIdentity);
        _ = await Assert.That(entry.Message).IsSameReferenceAs(message);
        _ = await Assert.That(entry.Outcome).IsSameReferenceAs(outcome);
        _ = await Assert.That(entry.Cause).IsNull();
        _ = await Assert.That(other.Outcome).IsNull();
        _ = await Assert.That(other.Cause).IsNull();
    }

    [Test]
    public async Task GivenDifferentResultTypesThenPreservesTypedOutcomes()
    {
        // Arrange
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> first = message.Yields<string>(TestData.ResultValue);
        Outcome<int> second = message.Yields<int>(TestData.DefaultValue);
        var subject = new InMemoryAuditor();
        Guid firstIdentity = await subject.Capture(message, CancellationToken.None);
        Guid secondIdentity = await subject.Capture(message, CancellationToken.None);

        // Act
        await subject.Complete(firstIdentity, first, CancellationToken.None);
        await subject.Complete(secondIdentity, second, CancellationToken.None);

        // Assert
        _ = await Assert.That(subject.Entries.Single(entry => entry.Identity == firstIdentity).Outcome).IsSameReferenceAs(first);
        _ = await Assert.That(subject.Entries.Single(entry => entry.Identity == secondIdentity).Outcome).IsSameReferenceAs(second);
    }

    [Test]
    public async Task GivenNullOutcomeThenThrowsArgumentNullExceptionAndPreservesEntry()
    {
        // Arrange
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = null!;
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        InMemoryAuditor.Entry original = subject.Entries.Single();

        // Act
        Exception? exception = await Capture(() => subject.Complete(identity, outcome, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(outcome));
        _ = await Assert.That(subject.Entries.Single()).IsSameReferenceAs(original);
    }

    [Test]
    public async Task GivenEmptyIdentityThenThrowsArgumentException()
    {
        // Arrange
        Guid identity = Guid.Empty;
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Complete(identity, outcome, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
        _ = await Assert.That(((ArgumentException)exception!).ParamName).IsEqualTo(nameof(identity));
        _ = await Assert.That(subject.Entries.IsEmpty).IsTrue();
    }

    [Test]
    public async Task GivenUnknownIdentityThenThrowsInvalidOperationException()
    {
        // Arrange
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Complete(TestData.Identity, outcome, CancellationToken.None));

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
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);

        if (failed)
        {
            await subject.Fail(new InvalidOperationException(TestData.FailureMessage), identity, CancellationToken.None);
        }
        else
        {
            await subject.Complete(identity, outcome, CancellationToken.None);
        }

        InMemoryAuditor.Entry original = subject.Entries.Single();

        // Act
        Exception? exception = await Capture(() => subject.Complete(identity, outcome, CancellationToken.None));

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
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        InMemoryAuditor.Entry original = subject.Entries.Single();
        await source.CancelAsync();

        // Act
        Exception? exception = await Capture(() => subject.Complete(identity, outcome, source.Token));

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