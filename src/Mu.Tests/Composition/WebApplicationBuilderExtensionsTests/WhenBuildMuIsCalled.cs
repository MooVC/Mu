namespace Mu.Composition.WebApplicationBuilderExtensionsTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Tracing;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenBuildMuIsCalled
{
    private const string AddCallback = "add";

    private const int DynamicPort = 0;

    private const string ExpectedCallbackOrder = "add,use";

    private const string UseCallback = "use";

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenIntegrationCallbacksThenRegistrationsRemainOpenUntilStartup(bool useContainerOverload)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        var callbacks = new List<string>();
        Container? integrationContainer = default;
        bool wasLocked = true;

        Action<SimpleInjectorAddOptions> add = options =>
        {
            callbacks.Add(AddCallback);
            integrationContainer = options.Container;
        };

        Action<SimpleInjectorUseOptions> use = _ =>
        {
            callbacks.Add(UseCallback);
            wasLocked = integrationContainer!.IsLocked;
        };

        // Act
        WebApplication application;
        Container expected;

        if (useContainerOverload)
        {
            application = builder.BuildMu(out expected, add: add, kestrel: ConfigureListener, use: use);
        }
        else
        {
            application = builder.BuildMu(add: add, kestrel: ConfigureListener, use: use);
            expected = builder.GetMuContainer()!;
        }

        await using (application)
        {
            bool wasLockedAfterBuild = expected.IsLocked;
            expected.Register<TestService>(Lifestyle.Scoped);
            await application.StartAsync();
            using DependencyScope scope = AsyncScopedLifestyle.BeginScope(expected);
            TestService service = expected.GetInstance<TestService>();

            // Assert
            _ = await Assert.That(string.Join(',', callbacks)).IsEqualTo(ExpectedCallbackOrder);
            _ = await Assert.That(integrationContainer).IsSameReferenceAs(expected);
            _ = await Assert.That(wasLocked).IsFalse();
            _ = await Assert.That(wasLockedAfterBuild).IsFalse();
            _ = await Assert.That(expected.IsLocked).IsTrue();
            _ = await Assert.That(service.Scribe).IsSameReferenceAs(expected.GetInstance<IScribe>());
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAssociatedContainerThenItsRegistrationsAndIntegrationAreReused(bool useHostBuilder)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        Container expected;

        if (useHostBuilder)
        {
            expected = builder.AddMu();
        }
        else
        {
            _ = builder.Services.AddMu(out expected);
        }

        var callbacks = new List<string>();
        Container? integrationContainer = default;

        // Act
        await using WebApplication host = builder.BuildMu(
            out Container result,
            add: options =>
            {
                callbacks.Add(AddCallback);
                integrationContainer = options.Container;
            },
            use: _ => callbacks.Add(UseCallback));
        result.Register<TestService>(Lifestyle.Scoped);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(expected);
        _ = await Assert.That(builder.GetMuContainer()).IsSameReferenceAs(expected);
        _ = await Assert.That(integrationContainer).IsSameReferenceAs(expected);
        _ = await Assert.That(host.GetMuContainer()).IsSameReferenceAs(expected);
        _ = await Assert.That(string.Join(',', callbacks)).IsEqualTo(ExpectedCallbackOrder);
        _ = await Assert.That(result.IsLocked).IsFalse();
    }

    [Test]
    public async Task GivenConfiguredContainerWhenAddCallbackIsProvidedThenItsOriginalIntegrationOptionsAreReused()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        SimpleInjectorAddOptions? initial = default;
        SimpleInjectorAddOptions? repeated = default;
        _ = builder.Services.AddMu(out Container expected, options => initial = options);

        // Act
        await using WebApplication host = builder.BuildMu(out Container result, add: options => repeated = options);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(expected);
        _ = await Assert.That(repeated).IsSameReferenceAs(initial);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenNullBuilderThenThrowsArgumentNullException(bool useContainerOverload)
    {
        // Arrange
        const string expectedMessage = "The builder for the Mu application must be provided.";
        WebApplicationBuilder builder = default!;

        // Act
        Exception? exception = Capture(() =>
        {
            if (useContainerOverload)
            {
                _ = builder.BuildMu(out _);
            }
            else
            {
                _ = builder.BuildMu();
            }
        });

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(builder));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenInvalidRegistrationsWhenStartupIsRequestedThenVerificationFailsBeforeTheServerStarts()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication host = builder.BuildMu(out Container container, kestrel: ConfigureListener);
        container.Register<InvalidService>(Lifestyle.Scoped);
        Exception? exception = default;

        // Act
        try
        {
            await host.StartAsync();
        }
        catch (Exception failure)
        {
            exception = failure;
        }

        IServer server = host.Services.GetRequiredService<IServer>();
        ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;

        // Assert
        _ = await Assert.That(exception).IsNotNull();
        _ = await Assert.That(container.IsLocked).IsTrue();
        _ = await Assert.That(addresses).IsEmpty();
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

    private static void ConfigureListener(KestrelServerOptions options)
    {
        options.ListenAnyIP(DynamicPort, listener => listener.Protocols = HttpProtocols.Http2);
    }

    public sealed class InvalidService(IDisposable dependency)
    {
        public IDisposable Dependency { get; } = dependency;
    }

    public sealed class TestService(IScribe scribe)
    {
        public IScribe Scribe { get; } = scribe;
    }
}