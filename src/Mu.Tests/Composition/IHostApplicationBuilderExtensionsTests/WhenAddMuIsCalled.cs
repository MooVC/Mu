namespace Mu.Composition.IHostApplicationBuilderExtensionsTests;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mu.Auditing;
using Mu.Communications.Mediation;
using Mu.Communications.Mediation.Configuration;
using Mu.Communications.Messaging;
using Mu.Testing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenAddMuIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenApplicationHandlerWhenMediationOptionsAreConfiguredThenMediatorExecutesWithinScope(bool configureOptions)
    {
        // Arrange
        var subject = new HostApplicationBuilder();

        if (configureOptions)
        {
            var values = new Dictionary<string, string?>
            {
                [$"{nameof(MediationOptions)}:{nameof(MediationOptions.Type)}"] = nameof(MediatorType.InMemory),
            };
            _ = subject.Configuration.AddInMemoryCollection(values);
        }

        IHandler<TestQuery, string> handler = Substitute.For<IHandler<TestQuery, string>>();
        var useCase = new TestQuery(TestData.Identity, TestData.ProposedAt);
        Result<string> expected = TestData.ResultValue;
        _ = handler
            .Handle(Arg.Any<Intent<TestQuery>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(((Intent<TestQuery>)call[0]).Yields(expected)));

        // Act
        using Container container = subject.AddMu();
        container.Register<IHandler<TestQuery, string>>(() => handler, Lifestyle.Scoped);
        using IHost host = subject.Build();
        _ = host.Services.UseSimpleInjector(container);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IMediator mediator = container.GetInstance<IMediator>();
        IAuditor auditor = container.GetInstance<IAuditor>();
        Result<string> result = await mediator.Execute<TestQuery, string>(useCase, CancellationToken.None);

        // Assert
        _ = await Assert.That(mediator).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(auditor).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(result).IsSameReferenceAs(expected);
        _ = await handler.Received(1).Handle(
            Arg.Is<Intent<TestQuery>>(intent => ReferenceEquals(intent.UseCase, useCase)),
            CancellationToken.None);
    }

    [Test]
    public async Task GivenInvalidConfiguredMediatorTypeThenThrowsInvalidOperationException()
    {
        // Arrange
        const string invalidType = "UnknownMediator";
        var subject = new HostApplicationBuilder();
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(MediationOptions)}:{nameof(MediationOptions.Type)}"] = invalidType,
        };
        _ = subject.Configuration.AddInMemoryCollection(values);

        // Act
        Exception? exception = Capture(() => _ = subject.AddMu());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
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