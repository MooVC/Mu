namespace Mu.Modelling.Services.CreationalServiceTests;

using Mu.Modelling.State;
using Mu.Persistence;
using Mu.Testing;

public sealed class WhenExecuteIsCalled
{
    [Test]
    public async Task GivenRootSucceedsThenSavesReturnedAggregateAndReturnsIdentity()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var useCase = new MuTestData.TestCreational();
        var allocator = new MuTestData.TestAllocator();
        IRoot<MuTestData.TestAggregate, MuTestData.TestCreational> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestCreational>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();
        var fact = new MuTestData.TestFact();
        MuTestData.TestAggregate opened = MuTestData.CreateAggregateWithChanges(new Revision(), fact);

        _ = root
            .Apply(Arg.Any<MuTestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromResult<Result<MuTestData.TestAggregate>>(opened));

        var subject = new CreationalService<MuTestData.TestAggregate, Guid, MuTestData.TestCreational>(allocator, root, store);

        // Act
        Result<Guid> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(MuTestData.Identity);
        await store.Received(1).Save(opened, MuTestData.Identity, source.Token);
        _ = await Assert.That(allocator.Surrendered).IsEmpty();
    }

    [Test]
    public async Task GivenRootReturnsFailureThenSurrendersIdentityAndDoesNotSave()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var useCase = new MuTestData.TestCreational();
        var allocator = new MuTestData.TestAllocator();
        IRoot<MuTestData.TestAggregate, MuTestData.TestCreational> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestCreational>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();
        Result<MuTestData.TestAggregate> failure = MuTestData.CreateFailure();

        _ = root
            .Apply(Arg.Any<MuTestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromResult(failure));

        var subject = new CreationalService<MuTestData.TestAggregate, Guid, MuTestData.TestCreational>(allocator, root, store);

        // Act
        Result<Guid> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Failures).IsEquivalentTo(failure.Failures);
        _ = await Assert.That(allocator.Surrendered).IsEquivalentTo(new[] { MuTestData.Identity });
        await store.DidNotReceive().Save(Arg.Any<MuTestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenRootThrowsThenSurrendersIdentityAndRethrows()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        var useCase = new MuTestData.TestCreational();
        var allocator = new MuTestData.TestAllocator();
        IRoot<MuTestData.TestAggregate, MuTestData.TestCreational> root = Substitute.For<IRoot<MuTestData.TestAggregate, MuTestData.TestCreational>>();
        IWriteStore<MuTestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<MuTestData.TestAggregate, Guid>>();

        _ = root
            .Apply(Arg.Any<MuTestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromException<Result<MuTestData.TestAggregate>>(exception));

        var subject = new CreationalService<MuTestData.TestAggregate, Guid, MuTestData.TestCreational>(allocator, root, store);

        // Act
        Exception? result = await Capture(() => subject.Execute(useCase, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        _ = await Assert.That(allocator.Surrendered).IsEquivalentTo(new[] { MuTestData.Identity });
    }

    private static async Task<Exception?> Capture(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}