namespace Mu.Modelling.Integrity;

using System.ComponentModel.DataAnnotations;
using Mu.Modelling.Behavior;

/// <summary>
/// Defines asynchronous specifications for non-mutational use cases.
/// </summary>
/// <typeparam name="TIntent">The non-mutational use case type.</typeparam>
public interface ISpecification<in TIntent>
    where TIntent : NonMutational
{
    /// <summary>
    /// Enforces specification rules for a non-mutational intent.
    /// </summary>
    IAsyncEnumerable<ValidationResult> Enforce(TIntent intent, CancellationToken cancellationToken);
}