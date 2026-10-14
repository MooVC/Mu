namespace Mu.Composition.ContainerExtensionsTests;

using Microsoft.Extensions.Configuration;
using SimpleInjector;

public sealed class WhenRegisterOnceIsCalled
{
    private const string FailureMessage = "The registrar failed before completing.";

    [Test]
    public async Task GivenRepeatedCallsThenTheRegistrarRunsOnceWithoutLockingTheContainer()
    {
        // Arrange
        using Container container = CreateContainer(out RegistrationTracker tracker);
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        Container first = container.RegisterOnce<FirstRegistrar>(configuration);
        Container repeated = container.RegisterOnce<FirstRegistrar>(configuration);

        // Assert
        _ = await Assert.That(first).IsSameReferenceAs(container);
        _ = await Assert.That(repeated).IsSameReferenceAs(container);
        _ = await Assert.That(tracker.FirstCalls).IsEqualTo(1);
        _ = await Assert.That(tracker.Configuration).IsSameReferenceAs(configuration);
        _ = await Assert.That(container.IsLocked).IsFalse();
    }

    [Test]
    public async Task GivenDifferentRegistrarsThenEachRunsOnce()
    {
        // Arrange
        using Container container = CreateContainer(out RegistrationTracker tracker);
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        _ = container.RegisterOnce<FirstRegistrar>(configuration);
        _ = container.RegisterOnce<SecondRegistrar>(configuration);
        _ = container.RegisterOnce<SecondRegistrar>(configuration);
        _ = container.RegisterOnce<FirstRegistrar>(configuration);

        // Assert
        _ = await Assert.That(tracker.FirstCalls).IsEqualTo(1);
        _ = await Assert.That(tracker.SecondCalls).IsEqualTo(1);
    }

    [Test]
    public async Task GivenDifferentContainersThenEachRunsTheRegistrar()
    {
        // Arrange
        using Container first = CreateContainer(out RegistrationTracker firstTracker);
        using Container second = CreateContainer(out RegistrationTracker secondTracker);
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        _ = first.RegisterOnce<FirstRegistrar>(configuration);
        _ = second.RegisterOnce<FirstRegistrar>(configuration);

        // Assert
        _ = await Assert.That(firstTracker.FirstCalls).IsEqualTo(1);
        _ = await Assert.That(secondTracker.FirstCalls).IsEqualTo(1);
    }

    [Test]
    public async Task GivenFailedRegistrationWhenItIsRetriedThenOnlyTheSuccessfulCallIsRemembered()
    {
        // Arrange
        using Container container = CreateContainer(out RegistrationTracker tracker);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        tracker.Fail = true;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterOnce<FirstRegistrar>(configuration));
        tracker.Fail = false;
        _ = container.RegisterOnce<FirstRegistrar>(configuration);
        _ = container.RegisterOnce<FirstRegistrar>(configuration);

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(exception!.Message).IsEqualTo(FailureMessage);
        _ = await Assert.That(tracker.FirstCalls).IsEqualTo(2);
        _ = await Assert.That(container.IsLocked).IsFalse();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenNullContainerOrConfigurationThenThrowsArgumentNullException(bool nullContainer)
    {
        // Arrange
        const string configurationMessage = "The source for configuration values must be provided.";
        const string containerMessage = "The dependency injection container must be provided.";
        using Container configured = CreateContainer(out RegistrationTracker tracker);
        Container container = nullContainer ? default! : configured;
        IConfiguration configuration = nullContainer ? new ConfigurationBuilder().Build() : default!;
        string expectedParameter = nullContainer ? nameof(container) : nameof(configuration);
        string expectedMessage = nullContainer ? containerMessage : configurationMessage;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterOnce<FirstRegistrar>(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(expectedParameter);
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
        _ = await Assert.That(tracker.FirstCalls).IsEqualTo(0);
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

    private static Container CreateContainer(out RegistrationTracker tracker)
    {
        var container = new Container();
        tracker = new RegistrationTracker();
        container.ContainerScope.SetItem(typeof(RegistrationTracker), tracker);

        return container;
    }

    public sealed class FirstRegistrar : IRegistrar
    {
        public static void Register(IConfiguration configuration, Container container)
        {
            var tracker = (RegistrationTracker)container.ContainerScope.GetItem(typeof(RegistrationTracker))!;
            tracker.Configuration = configuration;
            tracker.FirstCalls++;

            if (tracker.Fail)
            {
                throw new InvalidOperationException(FailureMessage);
            }
        }
    }

    public sealed class RegistrationTracker
    {
        public IConfiguration? Configuration { get; set; }

        public bool Fail { get; set; }

        public int FirstCalls { get; set; }

        public int SecondCalls { get; set; }
    }

    public sealed class SecondRegistrar : IRegistrar
    {
        public static void Register(IConfiguration configuration, Container container)
        {
            var tracker = (RegistrationTracker)container.ContainerScope.GetItem(typeof(RegistrationTracker))!;
            tracker.Configuration = configuration;
            tracker.SecondCalls++;
        }
    }
}