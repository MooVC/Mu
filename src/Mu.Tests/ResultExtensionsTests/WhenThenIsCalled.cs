namespace Mu.ResultExtensionsTests;

using Mu.Testing;

public sealed class WhenThenIsCalled
{
    [Test]
    public async Task GivenSuccessfulResultTaskThenInvokesSuccessAction()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(TestData.ResultValue);
        string? observed = default;

        // Act
        Result<string> result = await subject.Then(value =>
        {
            observed = value;

            return Task.CompletedTask;
        });

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(TestData.ResultValue);
        _ = await Assert.That(observed).IsEqualTo(TestData.ResultValue);
    }

    [Test]
    public async Task GivenUnsuccessfulResultTaskThenDoesNotInvokeSuccessAction()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(TestData.CreateFailure());
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