namespace Mu.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public static partial class ITransformExtensions
{
    public static TAggregate ApplyAll<TAggregate, TFact>(this IEnumerable<ITransform<TAggregate, TFact>> transforms, TAggregate aggregate, TFact fact)
        where TAggregate : Aggregate
        where TFact : Fact
    {
        foreach (ITransform<TAggregate, TFact> transform in transforms)
        {
            aggregate = transform.Apply(aggregate, fact);
        }

        return aggregate;
    }
}