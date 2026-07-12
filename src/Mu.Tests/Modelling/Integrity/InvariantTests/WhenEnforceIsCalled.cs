namespace Mu.Modelling.Integrity.InvariantTests;

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Mu.Testing;

public sealed class WhenEnforceIsCalled
{
    [Test]
    public async Task GivenAggregateAndMutationThenReturnsFailuresFromImplementation()
    {
        // Arrange
        ValidationResult failure = TestData.CreateFailure();
        var subject = new TestInvariant(failure);
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        var mutation = new TestData.TestMutation();

        // Act
        ValidationResult[] result = await ToArray(subject.Enforce(aggregate, mutation, CancellationToken.None));

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(new[] { failure });
        _ = await Assert.That(subject.Aggregates).IsEquivalentTo(new[] { aggregate });
        _ = await Assert.That(subject.Mutations).IsEquivalentTo(new[] { mutation });
    }

    [Test]
    public async Task GivenNullAggregateThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new TestInvariant();
        TestData.TestAggregate aggregate = null!;
        var mutation = new TestData.TestMutation();

        // Act
        Exception? exception = Capture(() => _ = subject.Enforce(aggregate, mutation, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenNullMutationThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new TestInvariant();
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        TestData.TestMutation mutation = null!;

        // Act
        Exception? exception = Capture(() => _ = subject.Enforce(aggregate, mutation, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }

    private static async Task<ValidationResult[]> ToArray(IAsyncEnumerable<ValidationResult> values)
    {
        var results = new List<ValidationResult>();

        await foreach (ValidationResult value in values)
        {
            results.Add(value);
        }

        return [.. results];
    }

    private sealed class TestInvariant(params ValidationResult[] failures)
        : Invariant<TestData.TestAggregate, TestData.TestMutation>
    {
        public IList<TestData.TestAggregate> Aggregates { get; } = [];

        public IList<TestData.TestMutation> Mutations { get; } = [];

        protected override async IAsyncEnumerable<ValidationResult> PerformEnforce(TestData.TestAggregate aggregate, TestData.TestMutation mutation, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Aggregates.Add(aggregate);
            Mutations.Add(mutation);

            foreach (ValidationResult failure in failures)
            {
                await Task.CompletedTask;

                yield return failure;
            }
        }
    }
}