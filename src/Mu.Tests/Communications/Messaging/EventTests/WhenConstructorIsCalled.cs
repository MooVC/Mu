namespace Mu.Communications.Messaging.EventTests;

using Mu.Communications.Tracing;
using Mu.Modelling.State;
using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Arrange
        var fact = new MuTestData.TestFact();
        Reference<Guid> origin = MuTestData.CreateReference();
        Ledger ledger = MuTestData.CreateLedger();

        // Act
        var result = new Event<MuTestData.TestFact, Guid>(MuTestData.CommittedAt, ledger, fact, origin, MuTestData.PreparedAt);

        // Assert
        _ = await Assert.That(result.CommittedAt).IsEqualTo(MuTestData.CommittedAt);
        _ = await Assert.That(result.Fact).IsSameReferenceAs(fact);
        _ = await Assert.That(((Event)result).Fact).IsSameReferenceAs(fact);
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.Origin).IsEqualTo(origin);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(MuTestData.PreparedAt);
    }

    [Test]
    public async Task GivenNullFactThenThrowsArgumentNullException()
    {
        // Arrange
        MuTestData.TestFact fact = null!;

        // Act
        Exception? exception = Capture(() => _ = new Event<MuTestData.TestFact, Guid>(
            MuTestData.CommittedAt,
            MuTestData.CreateLedger(),
            fact,
            MuTestData.CreateReference(),
            MuTestData.PreparedAt));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}