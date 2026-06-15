namespace Mu.Communications.PubSub;

using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;
using Mu.Modelling.State;

public sealed class IdentityConfirmationPolicy<TAggregate, TFact, TIdentity>(IAllocator<TIdentity> allocator)
    : IPolicy<TFact, TIdentity>
    where TAggregate : Aggregate, new()
    where TFact : Fact<TAggregate>
    where TIdentity : struct
{
    public async Task Handle(Event<TFact, TIdentity> @event, CancellationToken cancellationToken)
    {
        await allocator
            .Confirm(@event.Origin, cancellationToken)
            .ConfigureAwait(false);
    }
}