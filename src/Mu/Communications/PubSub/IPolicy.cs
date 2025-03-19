using Mu.Communications.Messaging;

namespace Mu.Communications.PubSub;

using Mu.Modelling.Behavior;

public interface IPolicy<TFact, TIdentity>
    where TFact : Fact
    where TIdentity : struct
{
    Task Handle(Event<TFact, TIdentity> @event, CancellationToken cancellationToken);
}