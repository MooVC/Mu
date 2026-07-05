namespace Mu.Modelling.Services.RootTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenPassingInvariantsThenProposesFactAndAppliesTransforms()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        var mutation = new MuTestData.TestMutation();
        var transform = new MuTestData.TestTransform();
        var subject = new Root<MuTestData.TestAggregate, MuTestData.TestFact, MuTestData.TestMutation>(
            [],
            [transform]);

        // Act
        Result<MuTestData.TestAggregate> result = await subject.Apply(aggregate, mutation, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Value!.Value).IsEqualTo(aggregate.Value + mutation.Value);
        _ = await Assert.That(result.Value.Propositions).HasSingleItem();
        _ = await Assert.That(transform.Facts[0].Value).IsEqualTo(mutation.Value);
    }

    [Test]
    public async Task GivenFailingInvariantThenReturnsFailures()
    {
        // Arrange
        ValidationResult failure = MuTestData.CreateFailure();
        var invariant = new MuTestData.TestInvariant<MuTestData.TestMutation>(failure);
        var subject = new Root<MuTestData.TestAggregate, MuTestData.TestFact, MuTestData.TestMutation>(
            [invariant],
            [new MuTestData.TestTransform()]);

        // Act
        Result<MuTestData.TestAggregate> result = await subject.Apply(MuTestData.CreateAggregate(), new MuTestData.TestMutation(), CancellationToken.None);

        // Assert
        ValidationResult actual = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(actual).IsSameReferenceAs(failure);
    }
}