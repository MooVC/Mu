namespace Mu.Communications.Ipc.Grpc.Composition.ContainerExtensionsTests;

using System.Net;
using System.Net.Sockets;
using global::Grpc.AspNetCore.Server;
using global::Grpc.Core;
using global::Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mu.Composition;
using Mu.Configuration;
using ProtoBuf.Grpc.Client;
using SimpleInjector;
using GrpcClientFixtures = Mu.Communications.Ipc.Grpc.Composition.ContainerExtensionsTests.WhenRegisterGrpcClientIsCalled;

public sealed class WhenRegisterGrpcHostIsCalled
{
    private const int DynamicPort = 0;

    private const string HostName = "localhost";

    private const string PayloadValue = "A message that exceeds the configured hosted message-size limit.";

    private const int ReceiveLimit = 1024;

    private const int SendLimit = 2048;

    private const int SmallMessageLimit = 1;

    [Test]
    public async Task GivenNullContainerThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The dependency injection container must be provided.";
        Container container = default!;
        var options = new Hosted.GrpcOptions();

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcHost<FirstService>(options));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(container));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenNullOptionsThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The hosted gRPC settings must be provided.";
        using var container = new Container();
        Hosted.GrpcOptions options = default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcHost<FirstService>(options));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(options));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenStandaloneContainerThenSettingsRemainAvailableAndRegistrationsRemainOpen()
    {
        // Arrange
        using var container = new Container();
        var options = new Hosted.GrpcOptions
        {
            EnableDetailedErrors = true,
            MaxReceiveMessageSize = ReceiveLimit,
            MaxSendMessageSize = SendLimit,
        };
        var marker = new FirstService();

        // Act
        Container result = container.RegisterGrpcHost<FirstService>(options);
        container.RegisterInstance(marker);
        GrpcServiceOptions<FirstService> settings = container.GetInstance<IOptions<GrpcServiceOptions<FirstService>>>().Value;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(settings.EnableDetailedErrors).IsTrue();
        _ = await Assert.That(settings.MaxReceiveMessageSize).IsEqualTo(ReceiveLimit);
        _ = await Assert.That(settings.MaxSendMessageSize).IsEqualTo(SendLimit);
        _ = await Assert.That(container.GetInstance<FirstService>()).IsSameReferenceAs(marker);
    }

    [Test]
    [Arguments(default, default)]
    [Arguments(ReceiveLimit, SendLimit)]
    public async Task GivenAttachedServicesThenServiceSettingsPreserveNullableMessageLimits(int? receiveLimit, int? sendLimit)
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new Hosted.GrpcOptions
        {
            EnableDetailedErrors = true,
            MaxReceiveMessageSize = receiveLimit,
            MaxSendMessageSize = sendLimit,
        };

        // Act
        _ = services.AddMu(out Container container, integration => _ = integration.Container.RegisterGrpcHost<FirstService>(options));
        using (container)
        {
            using ServiceProvider provider = services.BuildServiceProvider();
            GrpcServiceOptions<FirstService> settings = provider.GetRequiredService<IOptions<GrpcServiceOptions<FirstService>>>().Value;

            // Assert
            _ = await Assert.That(settings.EnableDetailedErrors).IsTrue();
            _ = await Assert.That(settings.MaxReceiveMessageSize).IsEqualTo(receiveLimit);
            _ = await Assert.That(settings.MaxSendMessageSize).IsEqualTo(sendLimit);
        }
    }

    [Test]
    public async Task GivenTwoHostedServicesWhenTheyShareAPortThenOnlyTheConfiguredListenerIsStarted()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.Services.AddMu(out Container container);
        using (container)
        {
            var options = new Hosted.GrpcOptions { Port = DynamicPort };
            _ = container.RegisterGrpcHost<FirstService>(options);
            _ = container.RegisterGrpcHost<SecondService>(options);
            _ = builder.ConfigureMu();
            await using WebApplication host = builder.Build();

            // Act
            await host.StartAsync();
            IServer server = host.Services.GetRequiredService<IServer>();
            ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;

            // Assert
            string address = await Assert.That(addresses).HasSingleItem();
            _ = await Assert.That(new Uri(address).Port).IsGreaterThan(DynamicPort);
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenTwoServicesAfterTheApplicationIsBuiltThenEachDistinctConfiguredPortIsStarted(bool sharePort)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication host = builder.BuildMu(out Container container);
        int firstPort = GetAvailablePort();
        int secondPort = sharePort ? firstPort : GetAvailablePort();

        while (!sharePort && secondPort == firstPort)
        {
            secondPort = GetAvailablePort();
        }

        var first = new Hosted.GrpcOptions { Port = firstPort };
        var second = new Hosted.GrpcOptions { Port = secondPort };

        // Act
        _ = container.RegisterGrpcHost<FirstService>(first);
        _ = container.RegisterGrpcHost<SecondService>(second);
        bool wasLocked = container.IsLocked;
        await host.StartAsync();
        IServer server = host.Services.GetRequiredService<IServer>();
        ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;
        int[] ports = addresses.Select(address => new Uri(address).Port).ToArray();

        // Assert
        _ = await Assert.That(wasLocked).IsFalse();
        _ = await Assert.That(builder.Services.IsReadOnly).IsTrue();
        _ = await Assert.That(ports.Length).IsEqualTo(sharePort ? 1 : 2);
        _ = await Assert.That(ports).Contains(firstPort);
        _ = await Assert.That(ports).Contains(secondPort);
    }

    [Test]
    [Arguments(default, default)]
    [Arguments(ReceiveLimit, SendLimit)]
    public async Task GivenBuiltApplicationThenServiceSettingsPreserveNullableMessageLimits(int? receiveLimit, int? sendLimit)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication host = builder.BuildMu(out Container container);
        var options = new Hosted.GrpcOptions
        {
            EnableDetailedErrors = true,
            MaxReceiveMessageSize = receiveLimit,
            MaxSendMessageSize = sendLimit,
            Port = DynamicPort,
        };

        // Act
        _ = container.RegisterGrpcHost<FirstService>(options);
        GrpcServiceOptions<FirstService> settings = host.Services.GetRequiredService<IOptions<GrpcServiceOptions<FirstService>>>().Value;

        // Assert
        _ = await Assert.That(settings.EnableDetailedErrors).IsTrue();
        _ = await Assert.That(settings.MaxReceiveMessageSize).IsEqualTo(receiveLimit);
        _ = await Assert.That(settings.MaxSendMessageSize).IsEqualTo(sendLimit);
        _ = await Assert.That(container.IsLocked).IsFalse();
    }

    [Test]
    [Arguments(SmallMessageLimit, default)]
    [Arguments(default, SmallMessageLimit)]
    public async Task GivenLateHostedMessageLimitsWhenAMessageExceedsTheConfiguredLimitThenTheCallIsRejected(int? receiveLimit, int? sendLimit)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication host = builder.BuildMu(out Container container);
        container.Register<GrpcClientFixtures.EchoService>(Lifestyle.Scoped);
        _ = container.RegisterGrpcHost<GrpcClientFixtures.EchoService>(new Hosted.GrpcOptions
        {
            MaxReceiveMessageSize = receiveLimit,
            MaxSendMessageSize = sendLimit,
            Port = DynamicPort,
        });
        _ = host.MapGrpcService<GrpcClientFixtures.EchoService>();
        await host.StartAsync();
        IServer server = host.Services.GetRequiredService<IServer>();
        string address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var endpoint = new UriBuilder(address) { Host = HostName };
        using GrpcChannel channel = GrpcChannel.ForAddress(endpoint.Uri);
        GrpcClientFixtures.IFirstContract client = channel.CreateGrpcService<GrpcClientFixtures.IFirstContract>();
        var request = new GrpcClientFixtures.Payload { Value = PayloadValue };
        RpcException? exception = default;

        // Act
        try
        {
            _ = await client.Execute(request);
        }
        catch (RpcException failure)
        {
            exception = failure;
        }

        // Assert
        _ = await Assert.That(exception).IsNotNull();
        _ = await Assert.That(exception!.StatusCode).IsEqualTo(StatusCode.ResourceExhausted);
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

    private static int GetAvailablePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, DynamicPort);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    public sealed class FirstService
    {
        public string Request { get; private set; } = string.Empty;

        public string Execute(string request)
        {
            Request = request;

            return Request;
        }
    }

    public sealed class SecondService
    {
        public string Request { get; private set; } = string.Empty;

        public string Execute(string request)
        {
            Request = request;

            return Request;
        }
    }
}