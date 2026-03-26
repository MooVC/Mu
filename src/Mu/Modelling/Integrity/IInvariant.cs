namespace Mu.Modelling.Integrity;

using System.ComponentModel.DataAnnotations;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Defines asynchronous invariant checks for mutational use cases.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type to validate.</typeparam>
/// <typeparam name="TIntent">The mutational use case type.</typeparam>
public interface IInvariant<in TAggregate, in TIntent>
    where TAggregate : Aggregate
    where TIntent : Mutational
{
    /// <summary>
    /// Enforces invariant rules for a mutation against an aggregate.
    /// </summary>
    IAsyncEnumerable<ValidationResult> Enforce(TAggregate aggregate, TIntent mutation, CancellationToken cancellationToken);
}