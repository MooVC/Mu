namespace Mu.Modelling.Services.RootTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenPassingInvariantsThenProposesFactAndAppliesTransforms()
    {
        // Arrange
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        var mutation = new TestData.TestMutation();
        var transform = new TestData.TestTransform();
        var subject = new Root<TestData.TestAggregate, TestData.TestFact, TestData.TestMutation>(
            [],
            [transform]);

        // Act
        Result<TestData.TestAggregate> result = await subject.Apply(aggregate, mutation, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Value!.Value).IsEqualTo(aggregate.Value + mutation.Value);
        _ = await Assert.That(result.Value.Propositions).HasSingleItem();
        _ = await Assert.That(transform.Facts[0].Value).IsEqualTo(mutation.Value);
    }

    [Test]
    public async Task GivenFailingInvariantThenReturnsFailures()
    {
        // Arrange
        ValidationResult failure = TestData.CreateFailure();
        var invariant = new TestData.TestInvariant<TestData.TestMutation>(failure);
        var subject = new Root<TestData.TestAggregate, TestData.TestFact, TestData.TestMutation>(
            [invariant],
            [new TestData.TestTransform()]);

        // Act
        Result<TestData.TestAggregate> result = await subject.Apply(TestData.CreateAggregate(), new TestData.TestMutation(), CancellationToken.None);

        // Assert
        ValidationResult actual = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(actual).IsSameReferenceAs(failure);
    }
}