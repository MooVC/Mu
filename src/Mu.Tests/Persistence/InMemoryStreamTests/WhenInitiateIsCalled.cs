namespace Mu.Persistence.InMemoryStreamTests;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Testing;

public sealed class WhenInitiateIsCalled
{
    [Test]
    public async Task GivenFactsThenStoresTypedEventsInRevisionOrder()
    {
        // Arrange
        var first = new TestData.TestFact();
        var second = new TestData.AlternateFact();
        var subject = new InMemoryStream<Guid>();

        // Act
        DateTimeOffset committedAt = await subject.Initiate([first, second], TestData.Identity, CancellationToken.None);
        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);

        // Assert
        _ = await Assert.That(events).Count().IsEqualTo(2);
        _ = await Assert.That(events[0]).IsTypeOf<Event<TestData.TestFact, Guid>>();
        _ = await Assert.That(events[1]).IsTypeOf<Event<TestData.AlternateFact, Guid>>();

        var firstEvent = (Event<TestData.TestFact, Guid>)events[0];
        var secondEvent = (Event<TestData.AlternateFact, Guid>)events[1];

        _ = await Assert.That(firstEvent.CommittedAt).IsEqualTo(committedAt);
        _ = await Assert.That(firstEvent.Fact).IsSameReferenceAs(first);
        _ = await Assert.That(firstEvent.Ledger.Causation).IsEqualTo(first.Identity);
        _ = await Assert.That(firstEvent.Ledger.Correlation).IsEqualTo(first.Identity);
        _ = await Assert.That(firstEvent.Origin.Identity).IsEqualTo(TestData.Identity);
        _ = await Assert.That(firstEvent.Origin.Revision).IsEqualTo(1ul);
        _ = await Assert.That(secondEvent.CommittedAt).IsEqualTo(committedAt);
        _ = await Assert.That(secondEvent.Fact).IsSameReferenceAs(second);
        _ = await Assert.That(secondEvent.Origin.Identity).IsEqualTo(TestData.Identity);
        _ = await Assert.That(secondEvent.Origin.Revision).IsEqualTo(2ul);
    }

    [Test]
    public async Task GivenExistingIdentityThenThrowsInvalidOperationException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None);

        // Act
        Exception? exception = await Capture(() => subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenNullFactsThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        IEnumerable<Fact> facts = null!;

        // Act
        Exception? exception = await Capture(() => subject.Initiate(facts, TestData.Identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenDefaultIdentityThenThrowsArgumentException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();

        // Act
        Exception? exception = await Capture(
            () => subject.Initiate([new TestData.TestFact()], Guid.Empty, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
    }

    [Test]
    public async Task GivenNullFactThenThrowsArgumentNullExceptionWithoutInitiatingStream()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        IEnumerable<Fact> facts = [null!];

        // Act
        Exception? exception = await Capture(() => subject.Initiate(facts, TestData.Identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();

        _ = await subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None);
        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);
        _ = await Assert.That(events).HasSingleItem();
    }

    [Test]
    public async Task GivenNoFactsThenStartsAtInitialRevision()
    {
        // Arrange
        var fact = new TestData.TestFact();
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([], TestData.Identity, CancellationToken.None);

        // Act
        _ = await subject.Append([fact], TestData.Identity, new(), CancellationToken.None);
        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);

        // Assert
        _ = await Assert.That(events).HasSingleItem();

        var @event = (Event<TestData.TestFact, Guid>)events[0];

        _ = await Assert.That(@event.Fact).IsSameReferenceAs(fact);
        _ = await Assert.That(@event.Origin.Revision).IsEqualTo(1ul);
    }

    [Test]
    public async Task GivenCancellationWhileWaitingThenThrowsOperationCanceledException()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1);
        var subject = new InMemoryStream<Guid>(semaphore);
        using var source = new CancellationTokenSource();
        Task<DateTimeOffset> operation = subject.Initiate([new TestData.TestFact()], TestData.Identity, source.Token);
        await source.CancelAsync();

        // Act
        Exception? exception = await Capture(() => operation);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<OperationCanceledException>();
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