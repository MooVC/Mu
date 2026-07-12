namespace Mu.ResultTests;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenSelectIsCalled
{
    private const int ProjectedValue = 6;

    [Test]
    public async Task GivenSuccessfulResultThenProjectsValue()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        Result<int> result = subject.Select(value => value.Length);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(ProjectedValue);
    }

    [Test]
    public async Task GivenSuccessfulResultWhenAsyncThenProjectsValue()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        Result<int> result = await subject.Select(value => Task.FromResult(value.Length));

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(ProjectedValue);
    }

    [Test]
    public async Task GivenSuccessfulResultWithBothSelectorsThenReturnsSuccessProjection()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        int result = subject.Select(_ => 0, value => value.Length);

        // Assert
        _ = await Assert.That(result).IsEqualTo(ProjectedValue);
    }

    [Test]
    public async Task GivenSuccessfulResultWithBothAsyncSelectorsThenReturnsSuccessProjection()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        int result = await subject.Select(_ => Task.FromResult(0), value => Task.FromResult(value.Length));

        // Assert
        _ = await Assert.That(result).IsEqualTo(ProjectedValue);
    }

    [Test]
    public async Task GivenUnsuccessfulResultThenPreservesFailures()
    {
        // Arrange
        ValidationResult failure = TestData.CreateFailure();
        Result<string> subject = failure;

        // Act
        Result<int> result = subject.Select(value => value.Length);

        // Assert
        ValidationResult actual = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(actual).IsSameReferenceAs(failure);
    }

    [Test]
    public async Task GivenUnsuccessfulResultWhenAsyncThenPreservesFailures()
    {
        // Arrange
        ValidationResult failure = TestData.CreateFailure();
        Result<string> subject = failure;

        // Act
        Result<int> result = await subject.Select(value => Task.FromResult(value.Length));

        // Assert
        ValidationResult actual = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(actual).IsSameReferenceAs(failure);
    }

    [Test]
    public async Task GivenUnsuccessfulResultWithBothSelectorsThenReturnsFailureProjection()
    {
        // Arrange
        Result<string> subject = TestData.CreateFailure();

        // Act
        int result = subject.Select(failures => failures.Length, value => value.Length);

        // Assert
        _ = await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task GivenUnsuccessfulResultWithBothAsyncSelectorsThenReturnsFailureProjection()
    {
        // Arrange
        Result<string> subject = TestData.CreateFailure();

        // Act
        int result = await subject.Select(failures => Task.FromResult(failures.Length), value => Task.FromResult(value.Length));

        // Assert
        _ = await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task GivenNullFailureSelectorThenThrowsArgumentNullException()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        Exception? exception = Capture(() => _ = subject.Select(default(Func<ImmutableArray<ValidationResult>, int>)!, value => value.Length));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    [Test]
    public async Task GivenNullSuccessSelectorThenThrowsArgumentNullException()
    {
        // Arrange
        Result<string> subject = TestData.ResultValue;

        // Act
        Exception? exception = Capture(() => _ = subject.Select(default(Func<string, int>)!));

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