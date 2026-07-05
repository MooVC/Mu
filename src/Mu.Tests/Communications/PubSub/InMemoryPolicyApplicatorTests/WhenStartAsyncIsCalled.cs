namespace Mu.Communications.PubSub.InMemoryPolicyApplicatorTests;

using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenStartAsyncIsCalled
{
    [Test]
    public async Task GivenSubscriberReceivesEventThenAppliesPolicy()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var subscriber = new MuTestData.TestSubscriber();
        var policy = new MuTestData.TestPolicy();
        IPolicyDirectory directory = Substitute.For<IPolicyDirectory>();
        IServiceScopeFactory factory = Substitute.For<IServiceScopeFactory>();
        IServiceScope scope = Substitute.For<IServiceScope>();
        Event<MuTestData.TestFact, Guid> @event = MuTestData.CreateEvent();

        _ = factory
            .CreateScope()
            .Returns(scope);

        _ = directory
            .Find(@event, Arg.Any<IServiceScope>())
            .Returns(new Policy<MuTestData.TestFact, Guid>([policy]));

        var subject = new InMemoryPolicyApplicator(directory, factory, [subscriber]);

        // Act
        await subject.StartAsync(source.Token);
        await subscriber.Raise(@event, source.Token);
        await subject.StopAsync(source.Token);
        await subscriber.Raise(@event, source.Token);

        // Assert
        _ = await Assert.That(policy.Events).HasSingleItem();
        _ = await Assert.That(subscriber.StartCount).IsEqualTo(1);
        _ = await Assert.That(subscriber.StopCount).IsEqualTo(1);
    }
}