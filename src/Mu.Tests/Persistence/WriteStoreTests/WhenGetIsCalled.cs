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
        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, new TestData.TestTransform());

        _ = stream
            .Find(Arg.Any<IStream<Guid>.FindOptions>(), source.Token)
            .Returns(Task.FromResult(ImmutableArray<Event>.Empty));

        // Act
        TestData.TestAggregate? result = await subject.Get(TestData.Identity, 3, source.Token);

        // Assert
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GivenEventsThenAppliesFactsAndReturnsAggregate()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IStream<Guid> stream = Substitute.For<IStream<Guid>>();
        var transform = new TestData.TestTransform();
        var first = new TestData.TestFact();
        var second = new TestData.TestFact(TestData.AlternateValue);
        ImmutableArray<Event> events = [TestData.CreateEvent(first), TestData.CreateEvent(second)];
        IStream<Guid>.FindOptions observed = null!;

        _ = stream
            .Find(Arg.Do<IStream<Guid>.FindOptions>(options => observed = options), source.Token)
            .Returns(Task.FromResult(events));

        var subject = new WriteStore<TestData.TestAggregate, Guid>(stream, transform);

        // Act
        TestData.TestAggregate? result = await subject.Get(TestData.Identity, 3, source.Token);

        // Assert
        _ = await Assert.That(result!.Value).IsEqualTo(TestData.DefaultValue + first.Value + second.Value);
        _ = await Assert.That(observed.Identity).IsEqualTo(TestData.Identity);
        _ = await Assert.That(observed.Revision.From).IsEqualTo(1ul);
        _ = await Assert.That(observed.Revision.To).IsEqualTo(3ul);
    }
}