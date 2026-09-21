namespace Mu.Modelling.Integrity;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Provides helpers to enforce collections of invariants.
/// </summary>
public static partial class IInvariantExtensions
{
    extension<TAggregate, TIntent>(IEnumerable<IInvariant<TAggregate, TIntent>> invariants)
        where TAggregate : Aggregate
        where TIntent : Mutational
    {
        /// <summary>
        /// Enforces all provided invariants and returns the collected failures.
        /// </summary>
        public async Task<ImmutableArray<ValidationResult>> EnforceAll(
            TAggregate aggregate,
            TIntent intent,
            CancellationToken cancellationToken)
        {
            var failures = new List<ValidationResult>();

            foreach (IInvariant<TAggregate, TIntent> invariant in invariants)
            {
                await foreach (ValidationResult failure in invariant.Enforce(aggregate, intent, cancellationToken))
                {
                    failures.AddRange(failure);
                }
            }

            return [.. failures];
        }
    }
}