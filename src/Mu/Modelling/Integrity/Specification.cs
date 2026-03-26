namespace Mu.Modelling.Integrity;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Mu.Modelling.Behavior;

/// <summary>
/// Base implementation for specifications over non-mutational use cases.
/// </summary>
/// <typeparam name="TIntent">The non-mutational use case type.</typeparam>
public abstract class Specification<TIntent>
    : ISpecification<TIntent>
    where TIntent : NonMutational
{
    /// <summary>
    /// Enforces the specification for the provided intent.
    /// </summary>
    public IAsyncEnumerable<ValidationResult> Enforce(TIntent intent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(intent);

        return PerformEnforce(intent, cancellationToken);
    }

    protected abstract IAsyncEnumerable<ValidationResult> PerformEnforce(TIntent intent, CancellationToken cancellationToken);
}