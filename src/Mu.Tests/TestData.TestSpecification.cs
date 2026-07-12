namespace Mu.Testing;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Mu.Modelling.Behavior;
using Mu.Modelling.Integrity;

public static partial class TestData
{
    public sealed class TestSpecification<TIntent>
        : Specification<TIntent>
        where TIntent : NonMutational
    {
        private readonly ImmutableArray<ValidationResult> _failures;

        public TestSpecification(params ValidationResult[] failures)
        {
            _failures = [.. failures];
        }

        public IList<TIntent> Intents { get; } = [];

        protected override async IAsyncEnumerable<ValidationResult> PerformEnforce(TIntent intent, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Intents.Add(intent);

            foreach (ValidationResult failure in _failures)
            {
                await Task.CompletedTask;

                yield return failure;
            }
        }
    }
}