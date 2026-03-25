namespace Mu.Communications.PubSub;

using Mu.Communications.Messaging;

/// <summary>
/// Publishes domain events to subscribers.
/// </summary>
public interface IPublisher
{
    Task Publish(CancellationToken cancellationToken, params IEnumerable<Event> events);
}
