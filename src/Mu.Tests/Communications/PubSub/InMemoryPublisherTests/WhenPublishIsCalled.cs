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
        Event @event = TestData.CreateEvent();
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
        const string expectedHeading = "The following events failed to be published to the channel:";
        Event @event = TestData.CreateEvent();
        string expectedMessage = $"{expectedHeading}{Environment.NewLine}{Environment.NewLine}{@event}";
        var channel = Channel.CreateUnbounded<Event>();
        channel.Writer.Complete();
        var subject = new InMemoryPublisher(channel.Writer);

        // Act
        Exception? exception = Capture(() => subject.Publish(CancellationToken.None, @event));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(expectedMessage);
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