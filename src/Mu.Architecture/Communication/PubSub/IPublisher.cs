namespace Mu.Architecture.Communication.PubSub;

using Mu.Architecture.Messaging;

public interface IPublisher
{
    Task Publish(CancellationToken cancellationToken, params IEnumerable<Event> events);
}