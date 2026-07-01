namespace Mu.Communications.PubSub;

using Mu.Communications.Messaging;

public interface IPolicy
{
    Task Apply(Event @event, CancellationToken cancellationToken);
}