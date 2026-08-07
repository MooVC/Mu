namespace Mu.Modelling.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Provides helpers to apply one or more transforms to an aggregate across a sequence of facts.
/// </summary>
public static partial class ITransformExtensions
{
    /// <summary>
    /// Applies one transform over a sequence of facts.
    /// </summary>
    public static Aggregate ApplyAll(this ITransform transform, Aggregate aggregate, params IEnumerable<Fact> facts)
    {
        IEnumerable<ITransform> transforms = [transform];

        return transforms.ApplyAll(aggregate, facts);
    }

    /// <summary>
    /// Applies multiple transforms over a sequence of facts.
    /// </summary>
    public static Aggregate ApplyAll(this IEnumerable<ITransform> transforms, Aggregate aggregate, params IEnumerable<Fact> facts)
    {
        foreach (Fact fact in facts)
        {
            foreach (ITransform transform in transforms)
            {
                aggregate = transform.Apply(aggregate, fact);
            }
        }

        return aggregate;
    }

    /// <summary>
    /// Applies one transform over a sequence of facts.
    /// </summary>
    public static TAggregate ApplyAll<TAggregate>(this ITransform<TAggregate> transform, TAggregate aggregate, params IEnumerable<Fact> facts)
        where TAggregate : Aggregate
    {
        IEnumerable<ITransform<TAggregate>> transforms = [transform];

        return transforms.ApplyAll(aggregate, facts);
    }

    /// <summary>
    /// Applies multiple transforms over a sequence of facts.
    /// </summary>
    public static TAggregate ApplyAll<TAggregate>(this IEnumerable<ITransform<TAggregate>> transforms, TAggregate aggregate, params IEnumerable<Fact> facts)
        where TAggregate : Aggregate
    {
        foreach (Fact fact in facts)
        {
            foreach (ITransform<TAggregate> transform in transforms)
            {
                aggregate = transform.Apply(aggregate, fact);
            }
        }

        return aggregate;
    }

    /// <summary>
    /// Applies one transform over a sequence of facts.
    /// </summary>
    public static TAggregate ApplyAll<TAggregate, TFact>(this ITransform<TAggregate, TFact> transform, TAggregate aggregate, params IEnumerable<TFact> facts)
        where TAggregate : Aggregate
        where TFact : Fact
    {
        IEnumerable<ITransform<TAggregate, TFact>> transforms = [transform];

        return transforms.ApplyAll(aggregate, facts);
    }

    /// <summary>
    /// Applies multiple transforms over a sequence of facts.
    /// </summary>
    public static TAggregate ApplyAll<TAggregate, TFact>(this IEnumerable<ITransform<TAggregate, TFact>> transforms, TAggregate aggregate, params IEnumerable<TFact> facts)
        where TAggregate : Aggregate
        where TFact : Fact
    {
        foreach (TFact fact in facts)
        {
            foreach (ITransform<TAggregate, TFact> transform in transforms)
            {
                aggregate = transform.Apply(aggregate, fact);
            }
        }

        return aggregate;
    }
}