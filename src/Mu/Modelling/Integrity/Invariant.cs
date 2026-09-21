namespace Mu.Modelling.Integrity;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Ardalis.GuardClauses;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using static Mu.Modelling.Integrity.Invariant_Resources;

/// <summary>
/// Base implementation for invariant checks over mutational use cases.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type to validate.</typeparam>
/// <typeparam name="TIntent">The mutational use case type.</typeparam>
public abstract class Invariant<TAggregate, TIntent>
    : IInvariant<TAggregate, TIntent>
    where TAggregate : Aggregate
    where TIntent : Mutational
{
    /// <summary>
    /// Enforces the invariant for the provided aggregate and mutation.
    /// </summary>
    public IAsyncEnumerable<ValidationResult> Enforce(TAggregate aggregate, TIntent mutation, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(aggregate, message: EnforceAggregateRequired);
        _ = Guard.Against.Null(mutation, message: EnforceMutationRequired);

        return PerformEnforce(aggregate, mutation, cancellationToken);
    }

    protected abstract IAsyncEnumerable<ValidationResult> PerformEnforce(TAggregate aggregate, TIntent mutation, CancellationToken cancellationToken);
}