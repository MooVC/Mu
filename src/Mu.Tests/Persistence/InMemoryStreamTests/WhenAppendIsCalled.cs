namespace Mu.Persistence.InMemoryStreamTests;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Testing;

public sealed class WhenAppendIsCalled
{
    [Test]
    public async Task GivenCurrentRevisionThenAppendsFactsInRevisionOrder()
    {
        // Arrange
        var first = new TestData.TestFact();
        var second = new TestData.TestFact(TestData.AlternateValue);
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([first], TestData.Identity, CancellationToken.None);

        // Act
        DateTimeOffset committedAt = await subject.Append([second], TestData.Identity, TestData.CreateRevision(1), CancellationToken.None);
        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);

        // Assert
        _ = await Assert.That(events).Count().IsEqualTo(2);

        var appended = (Event<TestData.TestFact, Guid>)events[1];

        _ = await Assert.That(appended.CommittedAt).IsEqualTo(committedAt);
        _ = await Assert.That(appended.Fact).IsSameReferenceAs(second);
        _ = await Assert.That(appended.Origin.Identity).IsEqualTo(TestData.Identity);
        _ = await Assert.That(appended.Origin.Revision).IsEqualTo(2ul);
    }

    [Test]
    public async Task GivenMissingIdentityThenThrowsInvalidOperationException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();

        // Act
        Exception? exception = await Capture(
            () => subject.Append([new TestData.TestFact()], TestData.Identity, TestData.CreateRevision(1), CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenStaleRevisionThenThrowsInvalidOperationExceptionWithoutAppending()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None);

        // Act
        Exception? exception = await Capture(
            () => subject.Append([new TestData.TestFact()], TestData.Identity, new(), CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();

        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);
        _ = await Assert.That(events).HasSingleItem();
    }

    [Test]
    public async Task GivenNullFactsThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        IEnumerable<Fact> facts = null!;

        // Act
        Exception? exception = await Capture(() => subject.Append(facts, TestData.Identity, new(), CancellationToken.None));

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
            () => subject.Append([new TestData.TestFact()], Guid.Empty, new(), CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
    }

    [Test]
    public async Task GivenNullFactThenThrowsArgumentNullExceptionWithoutAppending()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None);
        IEnumerable<Fact> facts = [null!];

        // Act
        Exception? exception = await Capture(
            () => subject.Append(facts, TestData.Identity, TestData.CreateRevision(1), CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();

        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);
        _ = await Assert.That(events).HasSingleItem();
    }

    [Test]
    public async Task GivenNoFactsThenReturnsWithoutInitiatingStream()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();

        // Act
        DateTimeOffset committedAt = await subject.Append([], TestData.Identity, new(), CancellationToken.None);
        _ = await subject.Initiate([new TestData.TestFact()], TestData.Identity, CancellationToken.None);

        // Assert
        _ = await Assert.That(committedAt).IsGreaterThan(DateTimeOffset.MinValue);

        ImmutableArray<Event> events = await subject.Find(new() { Identity = TestData.Identity }, CancellationToken.None);
        _ = await Assert.That(events).HasSingleItem();
    }

    [Test]
    public async Task GivenCancellationWhileWaitingThenThrowsOperationCanceledException()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1);
        var subject = new InMemoryStream<Guid>(semaphore);
        using var source = new CancellationTokenSource();
        Task<DateTimeOffset> operation =
            subject.Append([new TestData.TestFact()], TestData.Identity, new(), source.Token);
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