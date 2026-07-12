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
        var fact = new TestData.TestFact();
        Reference<Guid> origin = TestData.CreateReference();
        Ledger ledger = TestData.CreateLedger();

        // Act
        var result = new Event<TestData.TestFact, Guid>(TestData.CommittedAt, ledger, fact, origin, TestData.PreparedAt);

        // Assert
        _ = await Assert.That(result.CommittedAt).IsEqualTo(TestData.CommittedAt);
        _ = await Assert.That(result.Fact).IsSameReferenceAs(fact);
        _ = await Assert.That(((Event)result).Fact).IsSameReferenceAs(fact);
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.Origin).IsEqualTo(origin);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(TestData.PreparedAt);
    }

    [Test]
    public async Task GivenNullFactThenThrowsArgumentNullException()
    {
        // Arrange
        TestData.TestFact fact = null!;

        // Act
        Exception? exception = Capture(() => _ = new Event<TestData.TestFact, Guid>(
            TestData.CommittedAt,
            TestData.CreateLedger(),
            fact,
            TestData.CreateReference(),
            TestData.PreparedAt));

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