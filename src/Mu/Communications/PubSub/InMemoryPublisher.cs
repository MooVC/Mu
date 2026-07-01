namespace Mu.Communications.PubSub;

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Mu.Communications.Messaging;

public sealed class InMemoryPublisher(ChannelWriter<Event> channel)
    : IPublisher
{
    public Task Publish(CancellationToken cancellationToken, params IEnumerable<Event> events)
    {
        string failures = string.Join(Environment.NewLine, events.Where(@event => !channel.TryWrite(@event)));

        if (!string.IsNullOrEmpty(failures))
        {
            throw new InvalidOperationException($"""
                The following events failed to be published to the channel:

                {failures}
                """);
        }

        return Task.CompletedTask;
    }
}