namespace Mu.Communications.Messaging.OutcomeTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToResultIsCalled
{
    [Test]
    public async Task GivenOutcomeThenReturnsResult()
    {
        // Arrange
        Result<string> value = TestData.ResultValue;
        var subject = new Outcome<string>(TestData.CreateLedger(), TestData.PreparedAt, value);

        // Act
        Result<string> result = subject;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(value);
    }
}