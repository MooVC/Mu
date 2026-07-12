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
        var allocator = new TestData.TestAllocator();
        var subject = new IdentityConfirmationPolicy<TestData.TestAggregate, TestData.TestFact, Guid>(allocator);
        Event<TestData.TestFact, Guid> @event = TestData.CreateEvent();

        // Act
        await subject.Apply(@event, source.Token);

        // Assert
        _ = await Assert.That(allocator.Confirmed).IsEquivalentTo(new[] { TestData.Identity });
    }
}