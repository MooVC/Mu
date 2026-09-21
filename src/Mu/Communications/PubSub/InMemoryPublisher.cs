namespace Mu.Communications.PubSub;

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using MooVC;
using Mu.Communications.Messaging;
using static Mu.Communications.PubSub.InMemoryPublisher_Resources;

public sealed class InMemoryPublisher(ChannelWriter<Event> channel)
    : IPublisher
{
    public Task Publish(CancellationToken cancellationToken, params IEnumerable<Event> events)
    {
        string failures = string.Join(Environment.NewLine, events.Where(@event => !channel.TryWrite(@event)));

        if (!string.IsNullOrEmpty(failures))
        {
            throw new InvalidOperationException(PublishEventsAcceptedRequired.Format(Environment.NewLine, failures));
        }

        return Task.CompletedTask;
    }
}