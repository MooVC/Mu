namespace Mu.Modelling.Behavior;

using System;
using Mu.Modelling;
using Mu.Modelling.State;
using ProtoBuf;

/// <summary>
/// Represents a domain fact bound to a specific aggregate model.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type associated with the fact.</typeparam>
[ProtoContract]
public abstract record Fact<TAggregate>
    : Fact
    where TAggregate : Aggregate
{
    private static readonly Representation _model = typeof(TAggregate);

    protected Fact()
    {
    }

    protected Fact(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    /// <summary>
    /// Gets the model metadata associated with the fact.
    /// </summary>
    public override Representation Model => _model;
}