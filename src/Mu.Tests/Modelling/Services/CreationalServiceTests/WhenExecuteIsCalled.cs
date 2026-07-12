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
        var useCase = new TestData.TestCreational();
        var allocator = new TestData.TestAllocator();
        IRoot<TestData.TestAggregate, TestData.TestCreational> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestCreational>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();
        var fact = new TestData.TestFact();
        TestData.TestAggregate opened = TestData.CreateAggregateWithChanges(new Revision(), fact);

        _ = root
            .Apply(Arg.Any<TestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromResult<Result<TestData.TestAggregate>>(opened));

        var subject = new CreationalService<TestData.TestAggregate, Guid, TestData.TestCreational>(allocator, root, store);

        // Act
        Result<Guid> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(TestData.Identity);
        await store.Received(1).Save(opened, TestData.Identity, source.Token);
        _ = await Assert.That(allocator.Surrendered).IsEmpty();
    }

    [Test]
    public async Task GivenRootReturnsFailureThenSurrendersIdentityAndDoesNotSave()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var useCase = new TestData.TestCreational();
        var allocator = new TestData.TestAllocator();
        IRoot<TestData.TestAggregate, TestData.TestCreational> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestCreational>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();
        Result<TestData.TestAggregate> failure = TestData.CreateFailure();

        _ = root
            .Apply(Arg.Any<TestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromResult(failure));

        var subject = new CreationalService<TestData.TestAggregate, Guid, TestData.TestCreational>(allocator, root, store);

        // Act
        Result<Guid> result = await subject.Execute(useCase, source.Token);

        // Assert
        _ = await Assert.That(result.Failures).IsEquivalentTo(failure.Failures);
        _ = await Assert.That(allocator.Surrendered).IsEquivalentTo(new[] { TestData.Identity });
        await store.DidNotReceive().Save(Arg.Any<TestData.TestAggregate>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenRootThrowsThenSurrendersIdentityAndRethrows()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        var useCase = new TestData.TestCreational();
        var allocator = new TestData.TestAllocator();
        IRoot<TestData.TestAggregate, TestData.TestCreational> root = Substitute.For<IRoot<TestData.TestAggregate, TestData.TestCreational>>();
        IWriteStore<TestData.TestAggregate, Guid> store = Substitute.For<IWriteStore<TestData.TestAggregate, Guid>>();

        _ = root
            .Apply(Arg.Any<TestData.TestAggregate>(), useCase, source.Token)
            .Returns(Task.FromException<Result<TestData.TestAggregate>>(exception));

        var subject = new CreationalService<TestData.TestAggregate, Guid, TestData.TestCreational>(allocator, root, store);

        // Act
        Exception? result = await Capture(() => subject.Execute(useCase, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        _ = await Assert.That(allocator.Surrendered).IsEquivalentTo(new[] { TestData.Identity });
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