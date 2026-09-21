namespace Mu.Modelling.Services;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Provides helpers to apply one or more transforms to an aggregate across a sequence of facts.
/// </summary>
public static partial class ITransformExtensions
{
    extension(ITransform transform)
    {
        /// <summary>
        /// Applies one transform over a sequence of facts.
        /// </summary>
        public Aggregate ApplyAll(Aggregate aggregate, params IEnumerable<Fact> facts)
        {
            IEnumerable<ITransform> transforms = [transform];

            return transforms.ApplyAll(aggregate, facts);
        }
    }

    extension(IEnumerable<ITransform> transforms)
    {
        /// <summary>
        /// Applies multiple transforms over a sequence of facts.
        /// </summary>
        public Aggregate ApplyAll(Aggregate aggregate, params IEnumerable<Fact> facts)
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
    }

    extension<TAggregate>(ITransform<TAggregate> transform)
        where TAggregate : Aggregate
    {
        /// <summary>
        /// Applies one transform over a sequence of facts.
        /// </summary>
        public TAggregate ApplyAll(TAggregate aggregate, params IEnumerable<Fact> facts)
        {
            IEnumerable<ITransform<TAggregate>> transforms = [transform];

            return transforms.ApplyAll(aggregate, facts);
        }
    }

    extension<TAggregate>(IEnumerable<ITransform<TAggregate>> transforms)
        where TAggregate : Aggregate
    {
        /// <summary>
        /// Applies multiple transforms over a sequence of facts.
        /// </summary>
        public TAggregate ApplyAll(TAggregate aggregate, params IEnumerable<Fact> facts)
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
    }

    extension<TAggregate, TFact>(ITransform<TAggregate, TFact> transform)
        where TAggregate : Aggregate
        where TFact : Fact
    {
        /// <summary>
        /// Applies one transform over a sequence of facts.
        /// </summary>
        public TAggregate ApplyAll(TAggregate aggregate, params IEnumerable<TFact> facts)
        {
            IEnumerable<ITransform<TAggregate, TFact>> transforms = [transform];

            return transforms.ApplyAll(aggregate, facts);
        }
    }

    extension<TAggregate, TFact>(IEnumerable<ITransform<TAggregate, TFact>> transforms)
        where TAggregate : Aggregate
        where TFact : Fact
    {
        /// <summary>
        /// Applies multiple transforms over a sequence of facts.
        /// </summary>
        public TAggregate ApplyAll(TAggregate aggregate, params IEnumerable<TFact> facts)
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
}