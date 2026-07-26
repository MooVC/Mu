namespace Mu.Composition.IHostApplicationBuilderExtensionsTests;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;

public sealed class WhenUseMuIsCalled
{
    private const string ContainerRequiredMessage = "The dependency injection container must be provided.";
    private const string HostRequiredMessage = "The application host must be provided.";

    private interface IUnregisteredDependency
    {
    }

    [Test]
    public async Task GivenAConfiguredContainerThenContainerIsVerified()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();
        using Container container = root.AddMu();
        using IHost host = root.Build();

        // Act
        _ = host.UseMu(container);
        Exception? exception = Capture(() => container.Register<Subject>());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenAConfiguredHostThenReturnsSameHost()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();
        using Container container = root.AddMu();
        using IHost host = root.Build();

        // Act
        IHost result = host.UseMu(container);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(host);
    }

    [Test]
    public async Task GivenARegisteredServiceWithAFrameworkDependencyThenDependencyIsCrossWired()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();
        using Container container = root.AddMu();
        container.Register<Subject>();
        using IHost host = root.Build();

        // Act
        _ = host.UseMu(container);
        Subject result = container.GetInstance<Subject>();

        // Assert
        _ = await Assert.That(result.Logger).IsNotNull();
    }

    [Test]
    public async Task GivenAnInvalidContainerThenThrowsInvalidOperationException()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();
        using Container container = root.AddMu();
        container.Register<InvalidSubject>();
        using IHost host = root.Build();

        // Act
        Exception? exception = Capture(() => _ = host.UseMu(container));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenNullContainerThenThrowsArgumentNullException()
    {
        // Arrange
        IHost host = Substitute.For<IHost>();
        Container container = null!;

        // Act
        Exception? exception = Capture(() => _ = host.UseMu(container));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        var actual = (ArgumentNullException)exception!;
        _ = await Assert.That(actual.ParamName).IsEqualTo("container");
        _ = await Assert.That(actual.Message.StartsWith(ContainerRequiredMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenNullHostThenThrowsArgumentNullException()
    {
        // Arrange
        IHost host = null!;
        using var container = new Container();

        // Act
        Exception? exception = Capture(() => _ = host.UseMu(container));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        var actual = (ArgumentNullException)exception!;
        _ = await Assert.That(actual.ParamName).IsEqualTo("host");
        _ = await Assert.That(actual.Message.StartsWith(HostRequiredMessage, StringComparison.Ordinal)).IsTrue();
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

    private sealed record InvalidSubject(IUnregisteredDependency Dependency);

    private sealed class Subject(ILogger logger)
    {
        public ILogger Logger { get; } = logger;
    }
}