namespace Mu.Persistence.WriteStoreTests;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenGetIsCalled
{
    [Test]
    public async Task GivenNoEventsThenReturnsNull()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, new MuTestData.TestTransform());

        _ = stream
            .Find(Arg.Any<IStream<Guid>.FindOptions>(), source.Token)
            .Returns(Task.FromResult(ImmutableArray<Event>.Empty));

        // Act
        MuTestData.TestAggregate? result = await subject.Get(MuTestData.Identity, 3, source.Token);

        // Assert
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GivenEventsThenAppliesFactsAndReturnsAggregate()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var transform = new MuTestData.TestTransform();
        var first = new MuTestData.TestFact();
        var second = new MuTestData.TestFact(MuTestData.AlternateValue);
        ImmutableArray<Event> events = [MuTestData.CreateEvent(first), MuTestData.CreateEvent(second)];
        IStream<Guid>.FindOptions observed = null!;

        _ = stream
            .Find(Arg.Do<IStream<Guid>.FindOptions>(options => observed = options), source.Token)
            .Returns(Task.FromResult(events));

        var subject = new WriteStore<MuTestData.TestAggregate, Guid>(stream, transform);

        // Act
        MuTestData.TestAggregate? result = await subject.Get(MuTestData.Identity, 3, source.Token);

        // Assert
        _ = await Assert.That(result!.Value).IsEqualTo(MuTestData.DefaultValue + first.Value + second.Value);
        _ = await Assert.That(observed.Identity).IsEqualTo(MuTestData.Identity);
        _ = await Assert.That(observed.Revision.From).IsEqualTo(1ul);
        _ = await Assert.That(observed.Revision.To).IsEqualTo(3ul);
    }
}