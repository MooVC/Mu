namespace Mu.Modelling.State;

using Mu.Modelling.Behavior;

public static partial class AggregateExtensions
{
    public static TAggregate Propose<TAggregate>(this TAggregate aggregate, Fact<TAggregate> fact)
        where TAggregate : Aggregate
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        ArgumentNullException.ThrowIfNull(fact);

        return aggregate with
        {
            Propositions = [.. aggregate.Propositions, fact],
        };
    }
}