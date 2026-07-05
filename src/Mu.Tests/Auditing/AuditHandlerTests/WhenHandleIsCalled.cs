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
        IHandler<MuTestData.TestMutation, string> next = Substitute.For<IHandler<MuTestData.TestMutation, string>>();
        var intent = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, new MuTestData.TestMutation());
        Outcome<string> outcome = intent.Yields<string>(MuTestData.ResultValue);

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(MuTestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromResult(outcome));

        var subject = new AuditHandler<MuTestData.TestMutation, string>(auditor, next);

        // Act
        Outcome<string> result = await subject.Handle(intent, source.Token);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(outcome);
        await auditor.Received(1).Complete(MuTestData.Identity, outcome, source.Token);
        await auditor.DidNotReceive().Fail(Arg.Any<Exception>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenCaptureThrowsThenFailureIsNotAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<MuTestData.TestMutation, string> next = Substitute.For<IHandler<MuTestData.TestMutation, string>>();
        var intent = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, new MuTestData.TestMutation());

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromException<Guid>(exception));

        var subject = new AuditHandler<MuTestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await next.DidNotReceive().Handle(Arg.Any<Intent<MuTestData.TestMutation>>(), Arg.Any<CancellationToken>());
        await auditor.DidNotReceive().Fail(Arg.Any<Exception>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GivenNextHandlerThrowsThenFailureIsAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<MuTestData.TestMutation, string> next = Substitute.For<IHandler<MuTestData.TestMutation, string>>();
        var intent = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, new MuTestData.TestMutation());

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(MuTestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromException<Outcome<string>>(exception));

        var subject = new AuditHandler<MuTestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await auditor.Received(1).Fail(exception, MuTestData.Identity, source.Token);
    }

    [Test]
    public async Task GivenCompleteThrowsThenFailureIsAudited()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        var exception = new InvalidOperationException();
        IAuditor auditor = Substitute.For<IAuditor>();
        IHandler<MuTestData.TestMutation, string> next = Substitute.For<IHandler<MuTestData.TestMutation, string>>();
        var intent = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, new MuTestData.TestMutation());
        Outcome<string> outcome = intent.Yields<string>(MuTestData.ResultValue);

        _ = auditor
            .Capture(intent, source.Token)
            .Returns(Task.FromResult(MuTestData.Identity));

        _ = next
            .Handle(intent, source.Token)
            .Returns(Task.FromResult(outcome));

        _ = auditor
            .Complete(MuTestData.Identity, outcome, source.Token)
            .Returns(Task.FromException(exception));

        var subject = new AuditHandler<MuTestData.TestMutation, string>(auditor, next);

        // Act
        Exception? result = await Capture(() => subject.Handle(intent, source.Token));

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(exception);
        await auditor.Received(1).Fail(exception, MuTestData.Identity, source.Token);
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