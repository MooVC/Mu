namespace Mu.Communications.PubSub.InMemorySubscriberTests;

using System.Threading.Channels;
using Microsoft.Extensions.Logging.Abstractions;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenStartAsyncIsCalled
{
    [Test]
    public async Task GivenPublishedEventThenReceivedEventIsRaised()
    {
        // Arrange
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var channel = Channel.CreateUnbounded<Event>();
        Event @event = MuTestData.CreateEvent();
        var observed = new TaskCompletionSource<Event>(TaskCreationOptions.RunContinuationsAsynchronously);
        var subject = new InMemorySubscriber(channel.Reader, NullLogger<InMemorySubscriber>.Instance);

        subject.Received += (_, received, _) =>
        {
            observed.SetResult(received);

            return Task.CompletedTask;
        };

        await subject.StartAsync(source.Token);

        // Act
        await channel.Writer.WriteAsync(@event, source.Token);

        // Assert
        Event result = await observed.Task.WaitAsync(source.Token);
        _ = await Assert.That(result).IsSameReferenceAs(@event);

        channel.Writer.Complete();
        await subject.StopAsync(source.Token);
    }
}