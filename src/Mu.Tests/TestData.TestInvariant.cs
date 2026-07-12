namespace Mu.Testing;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Mu.Modelling.Behavior;
using Mu.Modelling.Integrity;

public static partial class TestData
{
    public sealed class TestInvariant<TMutation>
        : IInvariant<TestAggregate, TMutation>
        where TMutation : Mutational
    {
        private readonly ImmutableArray<ValidationResult> _failures;

        public TestInvariant(params ValidationResult[] failures)
        {
            _failures = [.. failures];
        }

        public IList<TestAggregate> Aggregates { get; } = [];

        public IList<TMutation> Mutations { get; } = [];

        public async IAsyncEnumerable<ValidationResult> Enforce(TestAggregate aggregate, TMutation mutation, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Aggregates.Add(aggregate);
            Mutations.Add(mutation);

            foreach (ValidationResult failure in _failures)
            {
                await Task.CompletedTask;

                yield return failure;
            }
        }
    }
}