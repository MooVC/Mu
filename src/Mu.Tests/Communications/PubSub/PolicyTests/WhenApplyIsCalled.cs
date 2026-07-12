namespace Mu.Communications.PubSub.PolicyTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenTypedEventThenAppliesAllPolicies()
    {
        // Arrange
        var first = new TestData.TestPolicy();
        var second = new TestData.TestPolicy();
        Event<TestData.TestFact, Guid> @event = TestData.CreateEvent();
        var subject = new Policy<TestData.TestFact, Guid>([first, second]);

        // Act
        await subject.Apply(@event, CancellationToken.None);

        // Assert
        _ = await Assert.That(first.Events).IsEquivalentTo(new[] { @event });
        _ = await Assert.That(second.Events).IsEquivalentTo(new[] { @event });
    }

    [Test]
    public async Task GivenWrongEventTypeThenThrowsInvalidCastException()
    {
        // Arrange
        Event @event = new Event<TestData.AlternateFact, Guid>(
            TestData.CommittedAt,
            TestData.CreateLedger(),
            new TestData.AlternateFact(),
            TestData.CreateReference(),
            TestData.PreparedAt);
        var subject = new Policy<TestData.TestFact, Guid>([]);

        // Act
        Exception? exception = await Capture(() => subject.Apply(@event, CancellationToken.None));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidCastException>();
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