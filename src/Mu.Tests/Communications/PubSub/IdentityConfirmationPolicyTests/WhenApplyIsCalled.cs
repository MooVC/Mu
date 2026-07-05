namespace Mu.Communications.PubSub.IdentityConfirmationPolicyTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenEventThenConfirmsOriginIdentity()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var allocator = new MuTestData.TestAllocator();
        var subject = new IdentityConfirmationPolicy<MuTestData.TestAggregate, MuTestData.TestFact, Guid>(allocator);
        Event<MuTestData.TestFact, Guid> @event = MuTestData.CreateEvent();

        // Act
        await subject.Apply(@event, source.Token);

        // Assert
        _ = await Assert.That(allocator.Confirmed).IsEquivalentTo(new[] { MuTestData.Identity });
    }
}