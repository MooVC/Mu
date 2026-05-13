namespace Mu.Modelling.Behavior;

using Mu.Modelling;
using Mu.Modelling.State;

/// <summary>
/// Represents a transitional use case targeting a specific aggregate reference.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type being transitioned.</typeparam>
/// <typeparam name="TIdentity">The identity type for the aggregate.</typeparam>
public abstract record Transitional<TAggregate, TIdentity>
    : Transitional
    where TAggregate : Aggregate
    where TIdentity : struct
{
    private static readonly Representation _model = typeof(TAggregate);

    protected Transitional(Reference<TIdentity> target)
    {
        Target = target;
    }

    protected Transitional(Guid identity, DateTimeOffset proposed, Reference<TIdentity> target)
        : base(identity, proposed)
    {
        Target = target;
    }

    /// <summary>
    /// Gets the aggregate reference targeted by the transition.
    /// </summary>
    public Reference<TIdentity> Target { get; }

    /// <summary>
    /// Gets the model metadata associated with the transitional use case.
    /// </summary>
    public override Representation Model => _model;
}