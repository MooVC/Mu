namespace Mu.Communications.PubSub.PolicyTests;

using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenTypedEventThenAppliesAllPolicies()
    {
        // Arrange
        var first = new MuTestData.TestPolicy();
        var second = new MuTestData.TestPolicy();
        Event<MuTestData.TestFact, Guid> @event = MuTestData.CreateEvent();
        var subject = new Policy<MuTestData.TestFact, Guid>([first, second]);

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
        Event @event = new Event<MuTestData.AlternateFact, Guid>(
            MuTestData.CommittedAt,
            MuTestData.CreateLedger(),
            new MuTestData.AlternateFact(),
            MuTestData.CreateReference(),
            MuTestData.PreparedAt);
        var subject = new Policy<MuTestData.TestFact, Guid>([]);

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