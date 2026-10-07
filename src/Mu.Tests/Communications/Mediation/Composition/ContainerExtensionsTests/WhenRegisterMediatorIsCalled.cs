namespace Mu.Communications.Mediation.Composition.ContainerExtensionsTests;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mu.Communications.Mediation.Configuration;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenRegisterMediatorIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenMissingOptionsThenRegistersDefaultScopedMediatorAndReturnsContainer(bool explicitlyNull)
    {
        // Arrange
        using Container subject = CreateContainer();

        // Act
        Container result = explicitlyNull
            ? subject.RegisterMediator(options: default)
            : subject.RegisterMediator();
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(subject);
        IMediator first = subject.GetInstance<IMediator>();
        IMediator second = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenInMemoryOptionsThenRegistersScopedMediatorAndReturnsContainer()
    {
        // Arrange
        using Container subject = CreateContainer();
        var options = new MediationOptions { Type = MediatorType.InMemory };

        // Act
        Container result = subject.RegisterMediator(options);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(subject);
        IMediator first = subject.GetInstance<IMediator>();
        IMediator second = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenUndefinedMediatorTypeThenRegistersDefaultMediator()
    {
        // Arrange
        const int undefinedType = int.MaxValue;
        using Container subject = CreateContainer();
        var options = new MediationOptions { Type = (MediatorType)undefinedType };

        // Act
        _ = subject.RegisterMediator(options);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(subject);
        IMediator result = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsTypeOf<InMemoryMediator>();
    }

    [Test]
    public async Task GivenEmptyConfigurationThenRegistersDefaultMediatorAndReturnsContainer()
    {
        // Arrange
        using Container subject = CreateContainer();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        Container result = subject.RegisterMediator(configuration);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(subject);
        IMediator mediator = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(mediator).IsTypeOf<InMemoryMediator>();
    }

    [Test]
    public async Task GivenConfiguredMediatorThenRegistersScopedMediatorAndReturnsContainer()
    {
        // Arrange
        using Container subject = CreateContainer();
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(MediationOptions)}:{nameof(MediationOptions.Type)}"] = nameof(MediatorType.InMemory),
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        Container result = subject.RegisterMediator(configuration);
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(subject);
        IMediator first = subject.GetInstance<IMediator>();
        IMediator second = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryMediator>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenMultipleScopesThenMediatorIsSharedWithinEachScope()
    {
        // Arrange
        using Container subject = CreateContainer();
        _ = subject.RegisterMediator();
        IMediator firstMediator;
        IMediator repeatedMediator;

        // Act
        using (DependencyScope first = AsyncScopedLifestyle.BeginScope(subject))
        {
            firstMediator = subject.GetInstance<IMediator>();
            repeatedMediator = subject.GetInstance<IMediator>();
        }

        using DependencyScope second = AsyncScopedLifestyle.BeginScope(subject);
        IMediator secondMediator = subject.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(firstMediator).IsSameReferenceAs(repeatedMediator);
        _ = await Assert.That(ReferenceEquals(firstMediator, secondMediator)).IsFalse();
    }

    [Test]
    public async Task GivenInvalidConfiguredTypeThenThrowsInvalidOperationException()
    {
        // Arrange
        const string invalidType = "UnknownMediator";
        using Container subject = CreateContainer();
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(MediationOptions)}:{nameof(MediationOptions.Type)}"] = invalidType,
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        Exception? exception = Capture(() => _ = subject.RegisterMediator(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenNullConfigurationThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The source for configuration values must be provided.";
        IConfiguration configuration = null!;
        using Container subject = CreateContainer();

        // Act
        Exception? exception = Capture(() => _ = subject.RegisterMediator(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(configuration));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
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

    private static Container CreateContainer()
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.RegisterInstance(Substitute.For<ILogger<InMemoryMediator>>());
        container.RegisterInstance<IServiceProvider>(container);

        return container;
    }
}