namespace Mu.ResultTests;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenWhenIsCalled
{
    [Test]
    public async Task GivenSuccessfulResultThenSuccessActionIsInvoked()
    {
        // Arrange
        Result<string> subject = MuTestData.ResultValue;
        string? observed = default;
        bool failed = false;

        // Act
        Result<string> result = subject.When(
            _ => failed = true,
            value => observed = value);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(failed).IsFalse();
        _ = await Assert.That(observed).IsEqualTo(MuTestData.ResultValue);
    }

    [Test]
    public async Task GivenSuccessfulResultWhenAsyncThenSuccessActionIsInvoked()
    {
        // Arrange
        Result<string> subject = MuTestData.ResultValue;
        string? observed = default;
        bool failed = false;

        // Act
        Result<string> result = await subject.When(
            _ =>
            {
                failed = true;

                return Task.CompletedTask;
            },
            value =>
            {
                observed = value;

                return Task.CompletedTask;
            });

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(failed).IsFalse();
        _ = await Assert.That(observed).IsEqualTo(MuTestData.ResultValue);
    }

    [Test]
    public async Task GivenUnsuccessfulResultThenFailureActionIsInvoked()
    {
        // Arrange
        ValidationResult failure = MuTestData.CreateFailure();
        Result<string> subject = failure;
        ImmutableArray<ValidationResult> observed = [];
        bool succeeded = false;

        // Act
        Result<string> result = subject.When(
            failures => observed = failures,
            _ => succeeded = true);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(succeeded).IsFalse();
        _ = await Assert.That(observed).IsEquivalentTo(new[] { failure });
    }

    [Test]
    public async Task GivenUnsuccessfulResultWhenAsyncThenFailureActionIsInvoked()
    {
        // Arrange
        ValidationResult failure = MuTestData.CreateFailure();
        Result<string> subject = failure;
        ImmutableArray<ValidationResult> observed = [];
        bool succeeded = false;

        // Act
        Result<string> result = await subject.When(
            failures =>
            {
                observed = failures;

                return Task.CompletedTask;
            },
            _ =>
            {
                succeeded = true;

                return Task.CompletedTask;
            });

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(succeeded).IsFalse();
        _ = await Assert.That(observed).IsEquivalentTo(new[] { failure });
    }
}