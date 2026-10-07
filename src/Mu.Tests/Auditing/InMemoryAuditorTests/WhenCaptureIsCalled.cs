namespace Mu.Auditing.InMemoryAuditorTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenCaptureIsCalled
{
    [Test]
    public async Task GivenMessageThenStoresPendingEntryWithUniqueIdentity()
    {
        // Arrange
        const int expectedCount = 2;
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();

        // Act
        Guid first = await subject.Capture(message, CancellationToken.None);
        Guid second = await subject.Capture(message, CancellationToken.None);

        // Assert
        InMemoryAuditor.Entry entry = subject.Entries.Single(candidate => candidate.Identity == first);
        _ = await Assert.That(first).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(second).IsNotEqualTo(first);
        _ = await Assert.That(subject.Entries.Length).IsEqualTo(expectedCount);
        _ = await Assert.That(entry.Message).IsSameReferenceAs(message);
        _ = await Assert.That(entry.Outcome).IsNull();
        _ = await Assert.That(entry.Cause).IsNull();
    }

    [Test]
    public async Task GivenConcurrentMessagesThenRetainsEveryEntryWithDistinctIdentity()
    {
        // Arrange
        const int count = 100;
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();

        // Act
        Guid[] identities = await Task.WhenAll(Enumerable.Range(0, count)
            .Select(_ => Task.Run(() => subject.Capture(message, CancellationToken.None))));

        // Assert
        _ = await Assert.That(identities.Distinct().Count()).IsEqualTo(count);
        _ = await Assert.That(subject.Entries.Select(entry => entry.Identity)).IsEquivalentTo(identities);
        _ = await Assert.That(subject.Entries.All(entry => ReferenceEquals(entry.Message, message))).IsTrue();
    }

    [Test]
    public async Task GivenNullMessageThenThrowsArgumentNullException()
    {
        // Arrange
        Message message = null!;
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Capture(message, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(message));
        _ = await Assert.That(subject.Entries.IsEmpty).IsTrue();
    }

    [Test]
    public async Task GivenCanceledTokenThenThrowsAndStoresNoEntry()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();

        // Act
        Exception? exception = await Capture(() => subject.Capture(message, source.Token));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<OperationCanceledException>();
        _ = await Assert.That(((OperationCanceledException)exception!).CancellationToken).IsEqualTo(source.Token);
        _ = await Assert.That(subject.Entries.IsEmpty).IsTrue();
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