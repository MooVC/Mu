namespace Mu.Communications.PubSub;

using System;
using MooVC;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using static Mu.Communications.PubSub.Policy_Resources;

internal sealed class Policy<TFact, TIdentity>(IEnumerable<IPolicy<TFact, TIdentity>> services)
    : IPolicy
    where TFact : Fact
    where TIdentity : struct
{
    public async Task Apply(Event @event, CancellationToken cancellationToken)
    {
        if (@event is not Event<TFact, TIdentity> expected)
        {
            throw new InvalidCastException(ApplyEventTypeRequired.Format(typeof(TFact).Name, typeof(TIdentity).Name));
        }

        IEnumerable<Task> policies = services.Select(service => service.Apply(expected, cancellationToken));

        await Task
            .WhenAll(policies)
            .ConfigureAwait(false);
    }
}