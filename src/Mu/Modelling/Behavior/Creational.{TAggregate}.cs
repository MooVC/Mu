namespace Mu.Modelling.Behavior;

using Mu.Modelling;
using Mu.Modelling.State;

/// <summary>
/// Represents a creational use case bound to a specific aggregate model.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type created by the use case.</typeparam>
public abstract record Creational<TAggregate>
    : Creational
    where TAggregate : Aggregate
{
    private static readonly Representation _model = typeof(TAggregate);

    protected Creational()
    {
    }

    protected Creational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    /// <summary>
    /// Gets the model metadata associated with the creational use case.
    /// </summary>
    public override Representation Model => _model;
}