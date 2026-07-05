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
        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, new MuTestData.TestTransform());

        // Act
        await subject.Save(MuTestData.CreateAggregate(), MuTestData.Identity, source.Token);

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
        var fact = new MuTestData.TestFact();
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregateWithChanges(new Revision(), fact);

        _ = stream
            .Initiate(Arg.Any<IEnumerable<Fact>>(), MuTestData.Identity, source.Token)
            .Returns(Task.FromResult(MuTestData.CommittedAt));

        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, new MuTestData.TestTransform());

        // Act
        await subject.Save(aggregate, MuTestData.Identity, source.Token);

        // Assert
        await stream.Received(1).Initiate(Arg.Is<IEnumerable<Fact>>(facts => facts.SequenceEqual(new[] { fact })), MuTestData.Identity, source.Token);
        await stream.DidNotReceive().Append(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<Revision>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenExistingAggregateWithChangesThenAppendsToStream()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        Revision revision = MuTestData.CreateRevision();
        var fact = new MuTestData.TestFact();
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregateWithChanges(revision, fact);

        _ = stream
            .Append(Arg.Any<IEnumerable<Fact>>(), MuTestData.Identity, revision, source.Token)
            .Returns(Task.FromResult(MuTestData.CommittedAt));

        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, new MuTestData.TestTransform());

        // Act
        await subject.Save(aggregate, MuTestData.Identity, source.Token);

        // Assert
        await stream.Received(1).Append(Arg.Is<IEnumerable<Fact>>(facts => facts.SequenceEqual(new[] { fact })), MuTestData.Identity, revision, source.Token);
        await stream.DidNotReceive().Initiate(Arg.Any<IEnumerable<Fact>>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenNullAggregateThenThrowsArgumentNullException()
    {
        // Arrange
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, new MuTestData.TestTransform());
        MuTestData.TestAggregate aggregate = null!;

        // Act
        Exception? exception = await Capture(() => subject.Save(aggregate, MuTestData.Identity, CancellationToken.None));

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