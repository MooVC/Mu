namespace Mu.Communications.PubSub.InMemoryPublisherTests;

using System.Threading.Channels;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenPublishIsCalled
{
    [Test]
    public async Task GivenEventsThenWritesEventsToChannel()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<Event>();
        Event @event = MuTestData.CreateEvent();
        var subject = new InMemoryPublisher(channel.Writer);

        // Act
        await subject.Publish(CancellationToken.None, @event);

        // Assert
        bool read = channel.Reader.TryRead(out Event? result);
        _ = await Assert.That(read).IsTrue();
        _ = await Assert.That(result).IsSameReferenceAs(@event);
    }

    [Test]
    public async Task GivenClosedChannelThenThrowsInvalidOperationException()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<Event>();
        channel.Writer.Complete();
        var subject = new InMemoryPublisher(channel.Writer);

        // Act
        Exception? exception = Capture(() => subject.Publish(CancellationToken.None, MuTestData.CreateEvent()));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
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