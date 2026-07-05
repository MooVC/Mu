namespace Mu.Communications.Messaging.OutcomeTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToResultIsCalled
{
    [Test]
    public async Task GivenOutcomeThenReturnsResult()
    {
        // Arrange
        Result<string> value = MuTestData.ResultValue;
        var subject = new Outcome<string>(MuTestData.CreateLedger(), MuTestData.PreparedAt, value);

        // Act
        Result<string> result = subject;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(value);
    }
}