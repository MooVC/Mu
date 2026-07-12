namespace Mu.Communications.Messaging.OutcomeTests;

using Mu.Communications.Tracing;
using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Arrange
        Ledger ledger = TestData.CreateLedger();
        Result<string> value = TestData.ResultValue;

        // Act
        var result = new Outcome<string>(ledger, TestData.PreparedAt, value);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(TestData.PreparedAt);
        _ = await Assert.That(result.Result).IsSameReferenceAs(value);
    }
}