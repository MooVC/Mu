namespace Mu.Persistence.InMemoryStreamTests;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenFindIsCalled
{
    [Test]
    public async Task GivenNoFiltersThenReturnsAllEventsInWriteOrder()
    {
        // Arrange
        var first = new TestData.TestFact();
        var second = new TestData.TestFact(TestData.AlternateValue);
        var subject = new InMemoryStream<Guid>();
        _ = await subject.Initiate([first], TestData.Identity, CancellationToken.None);
        _ = await subject.Initiate([second], TestData.AlternateIdentity, CancellationToken.None);

        // Act
        ImmutableArray<Event> result = await subject.Find(new(), CancellationToken.None);

        // Assert
        _ = await Assert.That(result).Count().IsEqualTo(2);
        _ = await Assert.That(result[0].Fact).IsSameReferenceAs(first);
        _ = await Assert.That(result[1].Fact).IsSameReferenceAs(second);
    }

    [Test]
    public async Task GivenFiltersThenReturnsEventsMatchingEveryInclusiveRange()
    {
        // Arrange
        var first = new TestData.TestFact();
        var expected = new TestData.AlternateFact();
        var other = new TestData.AlternateFact();
        var subject = new InMemoryStream<Guid>();
        DateTimeOffset committedAt = await subject.Initiate([first, expected], TestData.Identity, CancellationToken.None);
        _ = await subject.Initiate([other], TestData.AlternateIdentity, CancellationToken.None);

        var options = new IStream<Guid>.FindOptions
        {
            Committed = (committedAt, committedAt),
            Facts = [typeof(TestData.AlternateFact)],
            Identity = TestData.Identity,
            Proposed = (expected.Proposed, expected.Proposed),
            Revision = (2ul, 2ul),
        };

        // Act
        ImmutableArray<Event> result = await subject.Find(options, CancellationToken.None);

        // Assert
        _ = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(result[0].Fact).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task GivenNullOptionsThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new InMemoryStream<Guid>();
        IStream<Guid>.FindOptions options = null!;

        // Act
        Exception? exception = await Capture(() => subject.Find(options, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenCancellationWhileWaitingThenThrowsOperationCanceledException()
    {
        // Arrange
        using var semaphore = new SemaphoreSlim(0, 1);
        var subject = new InMemoryStream<Guid>(semaphore);
        using var source = new CancellationTokenSource();
        Task<ImmutableArray<Event>> operation = subject.Find(new(), source.Token);
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