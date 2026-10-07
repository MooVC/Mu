namespace Mu.Composition.IServiceCollectionExtensionsTests;

using Microsoft.Extensions.DependencyInjection;
using Mu.Auditing;
using Mu.Communications.Mediation;
using Mu.Communications.Mediation.Composition;
using Mu.Communications.Messaging;
using Mu.Communications.Tracing;
using Mu.Testing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenAddMuIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenApplicationHandlerWhenOptionsAreConfiguredThenMediatorExecutesWithinScope(bool configureOptions)
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddMu(out Container container, options: configureOptions ? _ => { }
        : default);
        _ = container.RegisterMediator();
        IHandler<TestQuery, string> handler = Substitute.For<IHandler<TestQuery, string>>();
        container.Register<IHandler<TestQuery, string>>(() => handler, Lifestyle.Scoped);

        using ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.UseSimpleInjector(container);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        using var source = new CancellationTokenSource();
        var useCase = new TestQuery(TestData.Identity, TestData.ProposedAt);
        Result<string> expected = TestData.ResultValue;

        _ = handler
            .Handle(Arg.Any<Intent<TestQuery>>(), source.Token)
            .Returns(call => Task.FromResult(((Intent<TestQuery>)call[0]).Yields(expected)));

        // Act
        IMediator mediator = container.GetInstance<IMediator>();
        Result<string> result = await mediator.Execute<TestQuery, string>(useCase, source.Token);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(expected);
        _ = await handler.Received(1).Handle(
            Arg.Is<Intent<TestQuery>>(intent => ReferenceEquals(intent.UseCase, useCase)),
            source.Token);
    }

    [Test]
    public async Task GivenApplicationRegistrationThenProviderResolvesItThroughContainer()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddMu(out Container container);
        IHandler<TestQuery, string> expected = Substitute.For<IHandler<TestQuery, string>>();
        container.RegisterInstance(expected);

        using ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.UseSimpleInjector(container);

        // Act
        IServiceProvider applicationProvider = container.GetInstance<IServiceProvider>();
        IHandler<TestQuery, string> result = applicationProvider.GetRequiredService<IHandler<TestQuery, string>>();

        // Assert
        _ = await Assert.That(applicationProvider).IsSameReferenceAs(container);
        _ = await Assert.That(result).IsSameReferenceAs(expected);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenConditionalMediatorWhenOptionsAreConfiguredThenMatchingRegistrationIsUsed(bool matches)
    {
        // Arrange
        var services = new ServiceCollection();
        IMediator expected = Substitute.For<IMediator>();
        Container? configured = default;

        // Act
        _ = services.AddMu(out Container container, options =>
        {
            configured = options.Container;
            Registration registration = Lifestyle.Singleton.CreateRegistration(() => expected, configured);
            configured.RegisterConditional<IMediator>(registration, _ => matches);
            _ = configured.RegisterMediator();
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.UseSimpleInjector(container);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IMediator result = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(configured).IsSameReferenceAs(container);

        if (matches)
        {
            _ = await Assert.That(result).IsSameReferenceAs(expected);
        }
        else
        {
            _ = await Assert.That(result).IsTypeOf<InMemoryMediator>();
        }
    }

    [Test]
    public async Task GivenMultipleScopesThenContextServicesAreSharedAndMediatorIsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddMu(out Container container);
        _ = container.RegisterMediator();
        using ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.UseSimpleInjector(container);

        IMediator firstMediator;
        IMediator repeatedMediator;
        IScopeManager firstManager;
        IScribe firstScribe;

        // Act
        using (DependencyScope first = AsyncScopedLifestyle.BeginScope(container))
        {
            firstMediator = container.GetInstance<IMediator>();
            repeatedMediator = container.GetInstance<IMediator>();
            firstManager = container.GetInstance<IScopeManager>();
            firstScribe = container.GetInstance<IScribe>();
        }

        using DependencyScope second = AsyncScopedLifestyle.BeginScope(container);
        IMediator secondMediator = container.GetInstance<IMediator>();
        IScopeManager secondManager = container.GetInstance<IScopeManager>();
        IScribe secondScribe = container.GetInstance<IScribe>();

        // Assert
        _ = await Assert.That(firstMediator).IsSameReferenceAs(repeatedMediator);
        _ = await Assert.That(ReferenceEquals(firstMediator, secondMediator)).IsFalse();
        _ = await Assert.That(firstManager).IsSameReferenceAs(secondManager);
        _ = await Assert.That(firstScribe).IsSameReferenceAs(secondScribe);
    }
}