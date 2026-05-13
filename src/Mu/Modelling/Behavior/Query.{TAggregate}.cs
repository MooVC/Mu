namespace Mu.Modelling.Behavior;

using Mu.Modelling;
using Mu.Modelling.State;

/// <summary>
/// Represents a query use case bound to a specific aggregate model.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type observed by the query.</typeparam>
public abstract record Query<TAggregate>
    : Query
    where TAggregate : Aggregate
{
    private static readonly Representation model = typeof(TAggregate);

    protected Query()
    {
    }

    protected Query(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    /// <summary>
    /// Gets the model metadata associated with the query.
    /// </summary>
    public override Representation Model => model;
}