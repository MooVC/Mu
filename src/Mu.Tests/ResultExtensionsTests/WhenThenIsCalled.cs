namespace Mu.ResultExtensionsTests;

using Mu.Testing;

public sealed class WhenThenIsCalled
{
    [Test]
    public async Task GivenSuccessfulResultTaskThenInvokesSuccessAction()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(MuTestData.ResultValue);
        string? observed = default;

        // Act
        Result<string> result = await subject.Then(value =>
        {
            observed = value;

            return Task.CompletedTask;
        });

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(MuTestData.ResultValue);
        _ = await Assert.That(observed).IsEqualTo(MuTestData.ResultValue);
    }

    [Test]
    public async Task GivenUnsuccessfulResultTaskThenDoesNotInvokeSuccessAction()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(MuTestData.CreateFailure());
        bool invoked = false;

        // Act
        Result<string> result = await subject.Then(_ =>
        {
            invoked = true;

            return Task.CompletedTask;
        });

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsFalse();
        _ = await Assert.That(invoked).IsFalse();
    }
}