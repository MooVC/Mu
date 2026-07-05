namespace Mu.Modelling.Integrity.IInvariantExtensionsTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenEnforceAllIsCalled
{
    [Test]
    public async Task GivenInvariantsThenCollectsAllFailures()
    {
        // Arrange
        ValidationResult first = MuTestData.CreateFailure();
        ValidationResult second = MuTestData.CreateFailure(MuTestData.AlternateFailureMessage);
        var firstInvariant = new MuTestData.TestInvariant<MuTestData.TestMutation>(first);
        var secondInvariant = new MuTestData.TestInvariant<MuTestData.TestMutation>(second);
        IInvariant<MuTestData.TestAggregate, MuTestData.TestMutation>[] invariants = [firstInvariant, secondInvariant];
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        var mutation = new MuTestData.TestMutation();

        // Act
        IReadOnlyCollection<ValidationResult> result = await invariants.EnforceAll(aggregate, mutation, CancellationToken.None);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(new[] { first, second });
        _ = await Assert.That(firstInvariant.Aggregates).IsEquivalentTo(new[] { aggregate });
        _ = await Assert.That(secondInvariant.Mutations).IsEquivalentTo(new[] { mutation });
    }
}