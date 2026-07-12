namespace Mu.Modelling.Integrity.IInvariantExtensionsTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenEnforceAllIsCalled
{
    [Test]
    public async Task GivenInvariantsThenCollectsAllFailures()
    {
        // Arrange
        ValidationResult first = TestData.CreateFailure();
        ValidationResult second = TestData.CreateFailure(TestData.AlternateFailureMessage);
        var firstInvariant = new TestData.TestInvariant<TestData.TestMutation>(first);
        var secondInvariant = new TestData.TestInvariant<TestData.TestMutation>(second);
        IInvariant<TestData.TestAggregate, TestData.TestMutation>[] invariants = [firstInvariant, secondInvariant];
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        var mutation = new TestData.TestMutation();

        // Act
        IReadOnlyCollection<ValidationResult> result = await invariants.EnforceAll(aggregate, mutation, CancellationToken.None);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(new[] { first, second });
        _ = await Assert.That(firstInvariant.Aggregates).IsEquivalentTo(new[] { aggregate });
        _ = await Assert.That(secondInvariant.Mutations).IsEquivalentTo(new[] { mutation });
    }
}