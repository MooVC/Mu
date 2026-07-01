namespace Mu.Communications.PubSub;

using System;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;

internal sealed class Policy<TFact, TIdentity>(IEnumerable<IPolicy<TFact, TIdentity>> services)
    : IPolicy
    where TFact : Fact
    where TIdentity : struct
{
    public Task Apply(Event @event, CancellationToken cancellationToken)
    {
        if (@event is not Event<TFact, TIdentity> expected)
        {
            throw new InvalidCastException($"Event is not of the type `Event<{typeof(TFact).Name}, {typeof(TIdentity).Name}>` expected by the Policy.");
        }

        return Parallel.ForEachAsync(services, cancellationToken, (service, cancellationToken) => new ValueTask(service.Apply(expected, cancellationToken)));
    }
}