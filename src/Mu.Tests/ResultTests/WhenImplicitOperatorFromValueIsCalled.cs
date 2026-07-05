namespace Mu.ResultTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorFromValueIsCalled
{
    [Test]
    public async Task GivenValueThenReturnsSuccessfulResult()
    {
        // Arrange
        string value = MuTestData.ResultValue;

        // Act
        Result<string> result = value;

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Value).IsEqualTo(value);
    }
}