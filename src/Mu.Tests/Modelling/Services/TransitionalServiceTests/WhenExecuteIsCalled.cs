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
        Reference<Guid> target = MuTestData.CreateReference(revision: 2);
        var useCase = new MuTestData.TestTransitional(target);
        IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();
        MuTestData.TestAggregate existing = MuTestData.CreateAggregateWithChanges(MuTestData.CreateRevision(2));
        MuTestData.TestAggregate updated = MuTestData.CreateAggregateWithChanges(MuTestData.CreateRevision(3), new MuTestData.TestFact());

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<MuTestData.TestAggregate?>(existing));

        _ = root
            .Apply(existing, useCase, source.Token)
            .Returns(Task.FromResult<Result<MuTestData.TestAggregate>>(updated));

        var subject = new TransitionalService<MuTestData.TestAggregate, Guid, MuTestData.TestTransitional>(root, store);

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
        Reference<Guid> target = MuTestData.CreateReference();
        var useCase = new MuTestData.TestTransitional(target);
        IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<MuTestData.TestAggregate?>(null));

        var subject = new TransitionalService<MuTestData.TestAggregate, Guid, MuTestData.TestTransitional>(root, store);

        // Act
        Result<Revision> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsFalse();
        await root.DidNotReceive().Apply(Arg.Any<MuTestData.TestAggregate>(), Arg.Any<MuTestData.TestTransitional>(), Arg.Any<CancellationToken>());
        await store.DidNotReceive().Save(Arg.Any<MuTestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenRootReturnsFailureThenDoesNotSave()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        Reference<Guid> target = MuTestData.CreateReference();
        var useCase = new MuTestData.TestTransitional(target);
        IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestTransitional>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();
        MuTestData.TestAggregate existing = MuTestData.CreateAggregate();
        Result<MuTestData.TestAggregate> failure = MuTestData.CreateFailure();

        _ = store
            .Get(target.Identity, target.Revision, source.Token)
            .Returns(Task.FromResult<MuTestData.TestAggregate?>(existing));

        _ = root
            .Apply(existing, useCase, source.Token)
            .Returns(Task.FromResult(failure));

        var subject = new TransitionalService<MuTestData.TestAggregate, Guid, MuTestData.TestTransitional>(root, store);

        // Act
        Result<Revision> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Failures).IsEquivalentTo(failure.Failures);
        await store.DidNotReceive().Save(Arg.Any<MuTestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}