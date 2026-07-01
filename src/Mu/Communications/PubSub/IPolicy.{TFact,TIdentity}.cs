namespace Mu.Communications.PubSub;

using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;

/// <summary>
/// Defines a subscription policy for handling published events.
/// </summary>
/// <typeparam name="TFact">The fact type handled by the policy.</typeparam>
/// <typeparam name="TIdentity">The aggregate identity type.</typeparam>
public interface IPolicy<TFact, TIdentity>
    where TFact : Fact
    where TIdentity : struct
{
    Task Apply(Event<TFact, TIdentity> @event, CancellationToken cancellationToken);
}