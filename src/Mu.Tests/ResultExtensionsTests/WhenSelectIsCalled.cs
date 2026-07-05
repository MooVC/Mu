namespace Mu.ResultExtensionsTests;

using Mu.Testing;

public sealed class WhenSelectIsCalled
{
    private const int ProjectedValue = 6;

    [Test]
    public async Task GivenSuccessfulResultTaskThenProjectsValue()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(MuTestData.ResultValue);

        // Act
        Result<int> result = await subject.Select(value => value.Length);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(ProjectedValue);
    }

    [Test]
    public async Task GivenSuccessfulResultTaskWhenAsyncThenProjectsValue()
    {
        // Arrange
        Task<Result<string>> subject = Task.FromResult<Result<string>>(MuTestData.ResultValue);

        // Act
        Result<int> result = await subject.Select(value => Task.FromResult(value.Length));

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(ProjectedValue);
    }
}