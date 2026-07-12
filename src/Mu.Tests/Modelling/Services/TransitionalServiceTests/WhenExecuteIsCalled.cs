namespace Mu.Modelling.Services.TransitionalServiceTests;

using Mu.Modelling.State;
using Mu.Persistence;
using Mu.Testing;

public sealed class WhenExecuteIsCalled
{
    [Test]
    public async Task GivenAggregateExistsAndRootSucceedsThenSavesReturnedAggregateAndReturnsRevision()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        Reference<Guid> target = TestData.CreateReference(revision: 2);
        var useCase = new TestData.TestTransitional(target);
        IRoot<TestData.TestAggregate, TestData.TestTransitional> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestTransitional>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();
        TestData.TestAggregate existing = TestData.CreateAggregateWithChanges(TestData.CreateRevision(2));
        TestData.TestAggregate updated = TestData.CreateAggregateWithChanges(TestData.CreateRevision(3), new TestData.TestFact());

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<TestData.TestAggregate?>(existing));

        _ = root
            .Apply(existing, useCase, source.Token)
            .Returns(Task.FromResult<Result<TestData.TestAggregate>>(updated));

        var subject = new TransitionalService<TestData.TestAggregate, Guid, TestData.TestTransitional>(root, store);

        // Act
        Result<Revision> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(updated.Revision);
        await store.Received(1).Save(updated, target.Identity, source.Token);
    }

    [Test]
    public async Task GivenAggregateDoesNotExistThenReturnsFailureAndDoesNotApplyRoot()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        Reference<Guid> target = TestData.CreateReference();
        var useCase = new TestData.TestTransitional(target);
        IRoot<TestData.TestAggregate, TestData.TestTransitional> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestTransitional>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<TestData.TestAggregate?>(null));

        var subject = new TransitionalService<TestData.TestAggregate, Guid, TestData.TestTransitional>(root, store);

        // Act
        Result<Revision> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsFalse();
        await root.DidNotReceive().Apply(Arg.Any<TestData.TestAggregate>(), Arg.Any<TestData.TestTransitional>(), Arg.Any<CancellationToken>());
        await store.DidNotReceive().Save(Arg.Any<TestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenRootReturnsFailureThenDoesNotSave()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        Reference<Guid> target = TestData.CreateReference();
        var useCase = new TestData.TestTransitional(target);
        IRoot<TestData.TestAggregate, TestData.TestTransitional> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestTransitional>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();
        TestData.TestAggregate existing = TestData.CreateAggregate();
        Result<TestData.TestAggregate> failure = TestData.CreateFailure();

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<TestData.TestAggregate?>(existing));

        _ = root
            .Apply(existing, useCase, source.Token)
            .Returns(Task.FromResult(failure));

        var subject = new TransitionalService<TestData.TestAggregate, Guid, TestData.TestTransitional>(root, store);

        // Act
        Result<Revision> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Failures).IsEquivalentTo(failure.Failures);
        await store.DidNotReceive().Save(Arg.Any<TestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}