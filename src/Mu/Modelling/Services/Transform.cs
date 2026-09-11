namespace Mu.Modelling.Services;

using System;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

internal sealed class Transform<TAggregate, TFact>(IEnumerable<ITransform<TAggregate, TFact>> transforms)
    : ITransform<TAggregate>
    where TAggregate : Aggregate
    where TFact : Fact
{
    public TAggregate Apply(TAggregate aggregate, Fact fact)
    {
        if (fact is not TFact expected)
        {
            throw new ArgumentException($"Expected fact of type {typeof(TFact)}, but received {fact.GetType()}.", nameof(fact));
        }

        return transforms.ApplyAll(aggregate, expected);
    }
}