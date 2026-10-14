namespace Mu.Composition.ContainerExtensionsTests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mu.Auditing;
using Mu.Auditing.Composition;
using Mu.Communications.Mediation;
using Mu.Communications.Mediation.Composition;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenRegisterDefaultsIsCalled
{
    [Test]
    public async Task GivenRepeatedCallsThenDefaultsAreSingletonAuditorAndScopedMediator()
    {
        // Arrange
        using Container container = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        Container result = container.RegisterDefaults(configuration);
        _ = container.RegisterDefaults(configuration);
        bool wasLocked = container.IsLocked;
        IAuditor firstAuditor;
        IMediator firstMediator;
        IMediator repeatedMediator;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            firstAuditor = container.GetInstance<IAuditor>();
            firstMediator = container.GetInstance<IMediator>();
            repeatedMediator = container.GetInstance<IMediator>();
        }

        using DependencyScope next = AsyncScopedLifestyle.BeginScope(container);
        IAuditor secondAuditor = container.GetInstance<IAuditor>();
        IMediator secondMediator = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(wasLocked).IsFalse();
        _ = await Assert.That(firstAuditor).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(secondAuditor).IsSameReferenceAs(firstAuditor);
        _ = await Assert.That(firstMediator).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(repeatedMediator).IsSameReferenceAs(firstMediator);
        _ = await Assert.That(ReferenceEquals(firstMediator, secondMediator)).IsFalse();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenCustomSharedServicesBeforeOrAfterDefaultsThenTheyArePreserved(bool registerAfterDefaults)
    {
        // Arrange
        using Container container = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IAuditor auditor = Substitute.For<IAuditor>();
        IMediator mediator = Substitute.For<IMediator>();

        if (!registerAfterDefaults)
        {
            container.RegisterInstance(auditor);
            container.RegisterInstance(mediator);
        }

        // Act
        _ = container.RegisterDefaults(configuration);

        if (registerAfterDefaults)
        {
            container.RegisterInstance(auditor);
            container.RegisterInstance(mediator);
        }

        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IAuditor actualAuditor = container.GetInstance<IAuditor>();
        IMediator actualMediator = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(actualAuditor).IsSameReferenceAs(auditor);
        _ = await Assert.That(actualMediator).IsSameReferenceAs(mediator);
    }

    [Test]
    public async Task GivenExistingFallbackRegistrationsThenTheyArePreservedWithoutDuplicateRegistrations()
    {
        // Arrange
        using Container container = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        _ = container.RegisterAuditor(configuration);
        _ = container.RegisterMediator(configuration);

        // Act
        _ = container.RegisterDefaults(configuration);
        _ = container.RegisterDefaults(configuration);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IAuditor auditor = container.GetInstance<IAuditor>();
        IMediator mediator = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(auditor).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(mediator).IsTypeOf<InMemoryMediator>();
    }

    [Test]
    public async Task GivenExistingUnregisteredTypeResolutionThenItsSharedServicesArePreserved()
    {
        // Arrange
        using Container container = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IAuditor expectedAuditor = Substitute.For<IAuditor>();
        IMediator expectedMediator = Substitute.For<IMediator>();
        Registration audit = Lifestyle.Singleton.CreateRegistration(() => expectedAuditor, container);
        Registration mediation = Lifestyle.Scoped.CreateRegistration(() => expectedMediator, container);
        container.ResolveUnregisteredType += (_, arguments) =>
        {
            if (arguments.UnregisteredServiceType == typeof(IAuditor))
            {
                arguments.Register(audit);
            }
            else if (arguments.UnregisteredServiceType == typeof(IMediator))
            {
                arguments.Register(mediation);
            }
        };

        // Act
        _ = container.RegisterDefaults(configuration);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IAuditor auditor = container.GetInstance<IAuditor>();
        IMediator mediator = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(auditor).IsSameReferenceAs(expectedAuditor);
        _ = await Assert.That(mediator).IsSameReferenceAs(expectedMediator);
    }

    [Test]
    public async Task GivenAConsumerSpecificMediatorThenGeneralConsumersUseTheDefaultAndTheSpecificConsumerKeepsItsMediator()
    {
        // Arrange
        using Container container = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        IMediator expected = Substitute.For<IMediator>();
        Registration registration = Lifestyle.Scoped.CreateRegistration(() => expected, container);
        container.RegisterConditional<IMediator>(registration, context => context.HasConsumer && context.Consumer.ImplementationType == typeof(SpecificConsumer));
        container.Register<GeneralConsumer>(Lifestyle.Scoped);
        container.Register<SpecificConsumer>(Lifestyle.Scoped);

        // Act
        _ = container.RegisterDefaults(configuration);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        IMediator general = container.GetInstance<GeneralConsumer>().Mediator;
        IMediator specific = container.GetInstance<SpecificConsumer>().Mediator;
        IMediator root = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(general).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(root).IsSameReferenceAs(general);
        _ = await Assert.That(specific).IsSameReferenceAs(expected);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenNullContainerOrConfigurationThenThrowsArgumentNullException(bool nullContainer)
    {
        // Arrange
        const string configurationMessage = "The source for configuration values must be provided.";
        const string containerMessage = "The dependency injection container must be provided.";
        using Container configured = CreateContainer();
        Container container = nullContainer ? default! : configured;
        IConfiguration configuration = nullContainer ? new ConfigurationBuilder().Build() : default!;
        string expectedParameter = nullContainer ? nameof(container) : nameof(configuration);
        string expectedMessage = nullContainer ? containerMessage : configurationMessage;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterDefaults(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(expectedParameter);
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();

            return default;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static Container CreateContainer()
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.RegisterInstance<IServiceProvider>(container);
        container.RegisterInstance(Substitute.For<ILogger<InMemoryMediator>>());

        return container;
    }

    public sealed record GeneralConsumer(IMediator Mediator);

    public sealed record SpecificConsumer(IMediator Mediator);
}