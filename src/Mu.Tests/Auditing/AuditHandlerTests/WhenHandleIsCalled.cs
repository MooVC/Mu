namespace Mu.Auditing.AuditHandlerTests;

using Mu.Communications.Mediation;
using Mu.Communications.Messaging;
using Mu.Testing;

public sealed class WhenHandleIsCalled
{
    [Test]
    public async Task GivenNextHandlerSucceedsThenCaptureAndCompleteAreCalled()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<TestData.TestMutation, string> next = Substitute.For<IHandler<TestData.TestMutation, string>>();
        var intent = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = intent.Yields<string>(TestData.ResultValue);

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(TestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromResult(outcome));

        var subject = new AuditHandler<TestData.TestMutation, string>(auditor, next);

        // Act
        Outcome<string> result = await subject.Handle(intent, source.Token);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(outcome);
        await auditor.Received(1).Complete(TestData.Identity, outcome, source.Token);
        await auditor.DidNotReceive().Fail(Arg.Any<Exception>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenCaptureThrowsThenFailureIsNotAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<TestData.TestMutation, string> next = Substitute.For<IHandler<TestData.TestMutation, string>>();
        var intent = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromException<Guid>(exception));

        var subject = new AuditHandler<TestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await next.DidNotReceive().Handle(Arg.Any<Intent<TestData.TestMutation>>(), Arg.Any<CancellationToken>());
        await auditor.DidNotReceive().Fail(Arg.Any<Exception>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenNextHandlerThrowsThenFailureIsAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<TestData.TestMutation, string> next = Substitute.For<IHandler<TestData.TestMutation, string>>();
        var intent = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(TestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromException<Outcome<string>>(exception));

        var subject = new AuditHandler<TestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await auditor.Received(1).Fail(exception, TestData.Identity, source.Token);
    }

    [Test]
    public async Task GivenCompleteThrowsThenFailureIsAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<TestData.TestMutation, string> next = Substitute.For<IHandler<TestData.TestMutation, string>>();
        var intent = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());
        Outcome<string> outcome = intent.Yields<string>(TestData.ResultValue);

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(TestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromResult(outcome));

        _ = auditor
            .Complete(TestData.Identity, outcome, source.Token)
            .Returns(Task.FromException(exception));

        var subject = new AuditHandler<TestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await auditor.Received(1).Fail(exception, TestData.Identity, source.Token);
    }

    private static async Task<Exception?> Capture(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}