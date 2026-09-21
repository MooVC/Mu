namespace Mu.Modelling.Services;

using System;
using MooVC;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using static Mu.Modelling.Services.Transform_Resources;

internal sealed class Transform<TAggregate, TFact>(IEnumerable<ITransform<TAggregate, TFact>> transforms)
    : ITransform<TAggregate>
    where TAggregate : Aggregate
    where TFact : Fact
{
    public TAggregate Apply(TAggregate aggregate, Fact fact)
    {
        if (fact is not TFact expected)
        {
            throw new ArgumentException(ApplyFactTypeRequired.Format(typeof(TFact), fact.GetType()), nameof(fact));
        }

        return transforms.ApplyAll(aggregate, expected);
    }
}