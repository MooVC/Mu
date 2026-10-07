namespace Mu.Auditing.InMemoryAuditorTests;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenEntriesIsCalled
{
    [Test]
    public async Task GivenNoCapturedMessagesThenReturnsEmptySnapshot()
    {
        // Arrange
        var subject = new InMemoryAuditor();

        // Act
        ImmutableArray<InMemoryAuditor.Entry> entries = subject.Entries;

        // Assert
        _ = await Assert.That(entries.IsEmpty).IsTrue();
        _ = await Assert.That(entries.IsDefault).IsFalse();
    }

    [Test]
    public async Task GivenSnapshotWhenEntriesChangeThenSnapshotIsPreserved()
    {
        // Arrange
        const int originalCount = 1;
        const int expectedCount = 2;
        var message = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        var subject = new InMemoryAuditor();
        Guid identity = await subject.Capture(message, CancellationToken.None);
        ImmutableArray<InMemoryAuditor.Entry> original = subject.Entries;
        Outcome<string> outcome = message.Yields<string>(TestData.ResultValue);

        // Act
        await subject.Complete(identity, outcome, CancellationToken.None);
        _ = await subject.Capture(message, CancellationToken.None);
        ImmutableArray<InMemoryAuditor.Entry> entries = subject.Entries;

        // Assert
        _ = await Assert.That(original.Length).IsEqualTo(originalCount);
        _ = await Assert.That(original.Single().Outcome).IsNull();
        _ = await Assert.That(entries.Length).IsEqualTo(expectedCount);
        _ = await Assert.That(entries.Single(entry => entry.Identity == identity).Outcome).IsSameReferenceAs(outcome);
    }
}