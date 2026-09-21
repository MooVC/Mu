namespace Mu.Modelling.State;

using Ardalis.GuardClauses;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;
using static Mu.Modelling.State.AggregateExtensions_Resources;

/// <summary>
/// Provides helpers for proposing facts against aggregates.
/// </summary>
public static partial class AggregateExtensions
{
    extension<TAggregate>(TAggregate aggregate)
        where TAggregate : Aggregate
    {
        /// <summary>
        /// Registers a fact proposition and applies matching transforms to derive the next aggregate state.
        /// </summary>
        public TAggregate Propose<TFact>(
            TFact fact,
            params IEnumerable<ITransform<TAggregate, TFact>> transforms)
            where TFact : Fact<TAggregate>
        {
            _ = Guard.Against.Null(aggregate, message: ProposeAggregateRequired);
            _ = Guard.Against.Null(fact, message: ProposeFactRequired);

            aggregate = aggregate with
            {
                Propositions = [.. aggregate.Propositions, fact],
            };

            return transforms.ApplyAll(aggregate, fact);
        }
    }
}