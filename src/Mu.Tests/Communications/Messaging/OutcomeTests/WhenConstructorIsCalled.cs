namespace Mu.Communications.Messaging.OutcomeTests;

using Mu.Communications.Tracing;
using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Arrange
        Ledger ledger = MuTestData.CreateLedger();
        Result<string> value = MuTestData.ResultValue;

        // Act
        var result = new Outcome<string>(ledger, MuTestData.PreparedAt, value);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(MuTestData.PreparedAt);
        _ = await Assert.That(result.Result).IsSameReferenceAs(value);
    }
}