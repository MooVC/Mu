namespace Mu.Modelling.State;

using Mu.Modelling.Behavior;
using Mu.Modelling.Services;

/// <summary>
/// Provides helpers for proposing facts against aggregates.
/// </summary>
public static partial class AggregateExtensions
{
    /// <summary>
    /// Registers a fact proposition and applies matching transforms to derive the next aggregate state.
    /// </summary>
    public static TAggregate Propose<TAggregate, TFact>(
        this TAggregate aggregate,
        TFact fact,
        params IEnumerable<ITransform<TAggregate, TFact>> transforms)
        where TAggregate : Aggregate
        where TFact : Fact<TAggregate>
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(fact);

        aggregate = aggregate with
        {
            Propositions = [.. aggregate.Propositions, fact],
        };

        return transforms.ApplyAll(aggregate, fact);
    }
}