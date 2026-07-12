namespace Mu.Persistence.WriteStoreTests;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using Mu.Testing;

public sealed class WhenSaveIsCalled
{
    [Test]
    public async Task GivenAggregateWithoutChangesThenDoesNotWriteToStream()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, new TestData.TestTransform());

        // Act
        await subject.Save(TestData.CreateAggregate(), TestData.Identity, source.Token);

        // Assert
        await stream.DidNotReceive().Append(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<Revision>(), Arg.Any<CancellationToken>());
        await stream.DidNotReceive().Initiate(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenNewAggregateWithChangesThenInitiatesStream()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var fact = new TestData.TestFact();
        TestData.TestAggregate aggregate = TestData.CreateAggregateWithChanges(new Revision(), fact);

        _ = stream
            .Initiate(Arg.Any<IEnumerable<Fact>>(), TestData.Identity, source.Token)
            .Returns(Task.FromResult(TestData.CommittedAt));

        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, new TestData.TestTransform());

        // Act
        await subject.Save(aggregate, TestData.Identity, source.Token);

        // Assert
        await stream.Received(1).Initiate(Arg.Is<IEnumerable<Fact>>(facts => facts.SequenceEqual(new[] { fact })), TestData.Identity, source.Token);
        await stream.DidNotReceive().Append(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<Revision>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenExistingAggregateWithChangesThenAppendsToStream()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        Revision revision = TestData.CreateRevision();
        var fact = new TestData.TestFact();
        TestData.TestAggregate aggregate = TestData.CreateAggregateWithChanges(revision, fact);

        _ = stream
            .Append(Arg.Any<IEnumerable<Fact>>(), TestData.Identity, revision, source.Token)
            .Returns(Task.FromResult(TestData.CommittedAt));

        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, new TestData.TestTransform());

        // Act
        await subject.Save(aggregate, TestData.Identity, source.Token);

        // Assert
        await stream.Received(1).Append(Arg.Is<IEnumerable<Fact>>(facts => facts.SequenceEqual(new[] { fact })), TestData.Identity, revision, source.Token);
        await stream.DidNotReceive().Initiate(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenNullAggregateThenThrowsArgumentNullException()
    {
        // Arrange
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, new TestData.TestTransform());
        TestData.TestAggregate aggregate = null!;

        // Act
        Exception? exception = await Capture(() => subject.Save(aggregate, TestData.Identity, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
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