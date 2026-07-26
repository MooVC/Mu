namespace Mu.Sample.Open.OpenGrpcServiceTests;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Mu.Communications.Mediation;
using Mu.Communications.Messaging;
using Mu.Sample.Account;
using OpenAccount = global::Mu.Sample.Open.Open;

public sealed class WhenExecuteIsCalled
{
    private const string FailureMessage = "The account could not be opened.";
    private const string OwnerName = "Grace Hopper";
    private static readonly Guid _accountIdentity = Guid.Parse("ad2247aa-d9bf-4ce2-a4e7-73d7c57b28df");

    [Test]
    public async Task GivenARequestThenForwardsCancellationToTheMediator()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IMediator mediator = Substitute.For<IMediator>();

        _ = mediator
            .Execute<OpenAccount, Guid>(Arg.Any<OpenAccount>(), source.Token)
            .Returns(Task.FromResult<Result<Guid>>(_accountIdentity));

        var service = new OpenGrpcService(mediator);
        var request = new OpenAccount(new Owner(OwnerName));

        // Act
        _ = await service.Execute(request, source.Token);

        // Assert
        _ = await mediator
            .Received(1)
            .Execute<OpenAccount, Guid>(Arg.Any<OpenAccount>(), source.Token);
    }

    [Test]
    public async Task GivenARequestThenTriggersTheHandler()
    {
        // Arrange
        using var source = new CancellationTokenSource();
        IHandler<OpenAccount, Guid> handler = Substitute.For<IHandler<OpenAccount, Guid>>();
        ILogger<InMemoryMediator> logger = Substitute.For<ILogger<InMemoryMediator>>();
        IServiceProvider provider = Substitute.For<IServiceProvider>();
        Result<Guid> outcome = _accountIdentity;

        _ = provider
            .GetService(typeof(IHandler<OpenAccount, Guid>))
            .Returns(handler);

        _ = handler
            .Handle(Arg.Any<Intent<OpenAccount>>(), source.Token)
            .Returns(callInfo => Task.FromResult(((Intent<OpenAccount>)callInfo[0]!).Yields(outcome)));

        var mediator = new InMemoryMediator(logger, provider);
        var service = new OpenGrpcService(mediator);
        var request = new OpenAccount(new Owner(OwnerName));

        // Act
        _ = await service.Execute(request, source.Token);

        // Assert
        _ = await handler
            .Received(1)
            .Handle(Arg.Is<Intent<OpenAccount>>(intent => intent!.UseCase.Owner.Name == OwnerName), source.Token);
    }

    [Test]
    public async Task GivenARequestWhenHandlerFailsThenReturnsFailures()
    {
        // Arrange
        IMediator mediator = Substitute.For<IMediator>();
        Result<Guid> outcome = new ValidationResult(FailureMessage);

        _ = mediator
            .Execute<OpenAccount, Guid>(Arg.Any<OpenAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(outcome));

        var service = new OpenGrpcService(mediator);
        var request = new OpenAccount(new Owner(OwnerName));

        // Act
        OpenResponse result = await service.Execute(request, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Successful).IsFalse();
        _ = await Assert.That(result.AccountId).IsEqualTo(string.Empty);
        string failure = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(failure).IsEqualTo(FailureMessage);
    }

    [Test]
    public async Task GivenARequestWhenHandlerSucceedsThenReturnsAccountIdentity()
    {
        // Arrange
        IMediator mediator = Substitute.For<IMediator>();

        _ = mediator
            .Execute<OpenAccount, Guid>(Arg.Is<OpenAccount>(useCase => useCase!.Owner.Name == OwnerName), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Guid>>(_accountIdentity));

        var service = new OpenGrpcService(mediator);
        var request = new OpenAccount(new Owner(OwnerName));

        // Act
        OpenResponse result = await service.Execute(request, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Successful).IsTrue();
        _ = await Assert.That(result.AccountId).IsEqualTo(_accountIdentity.ToString());
        _ = await Assert.That(result.Failures).IsEmpty();

        _ = await mediator
            .Received(1)
            .Execute<OpenAccount, Guid>(Arg.Is<OpenAccount>(useCase => useCase!.Owner.Name == OwnerName), Arg.Any<CancellationToken>());
    }
}