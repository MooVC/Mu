namespace Mu.Communications.Ipc.Grpc.Composition.ContainerExtensionsTests;

using global::Grpc.Core;
using global::Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Tracing;
using Mu.Composition;
using Mu.Configuration;
using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed class WhenRegisterGrpcClientIsCalled
{
    private const string FirstAddress = "http://localhost:50051";

    private const string HostName = "localhost";

    private const string PayloadValue = "A message that exceeds the configured message-size limit.";

    private const string SecondAddress = "http://localhost:50052";

    private const int SmallMessageLimit = 1;

    private const int TimeoutSeconds = 15;

    [Service]
    public interface IFirstContract
    {
        ValueTask<Payload> Execute(Payload request, CallContext context = default);
    }

    [Service]
    public interface ISecondContract
    {
        ValueTask<Payload> Execute(Payload request, CallContext context = default);
    }

    [Test]
    public async Task GivenNullContainerThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The dependency injection container must be provided.";
        Container container = default!;
        var options = new Remote.GrpcOptions();

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcClient<IFirstContract, FirstClient>(options, (proxy, scribe, timeout) => new(proxy, scribe, timeout)));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(container));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task GivenNullFactoryThenThrowsArgumentNullExceptionWithoutCreatingAChannel()
    {
        // Arrange
        const string expectedMessage = "The factory for the gRPC client must be provided.";
        using Container container = CreateContainer(out _);
        var options = new Remote.GrpcOptions();
        Func<IFirstContract, IScribe, TimeSpan, FirstClient> factory = default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcClient(options, factory));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(factory));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
        _ = await Assert.That(container.ContainerScope.GetDisposables().OfType<GrpcChannel>()).IsEmpty();
    }

    [Test]
    public async Task GivenNullOptionsThenThrowsArgumentNullExceptionWithoutCreatingAChannel()
    {
        // Arrange
        const string expectedMessage = "The remote gRPC settings must be provided.";
        using Container container = CreateContainer(out _);
        Remote.GrpcOptions options = default!;

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcClient<IFirstContract, FirstClient>(options, (proxy, scribe, timeout) => new(proxy, scribe, timeout)));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(options));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
        _ = await Assert.That(container.ContainerScope.GetDisposables().OfType<GrpcChannel>()).IsEmpty();
    }

    [Test]
    public async Task GivenALockedContainerWhenClientRegistrationFailsThenTheNewChannelIsDisposed()
    {
        // Arrange
        using Container container = CreateContainer(out _);
        var options = new Remote.GrpcOptions();
        _ = container.RegisterGrpcClient<IFirstContract, FirstClient>(options, (proxy, scribe, timeout) => new(proxy, scribe, timeout));
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        _ = container.GetInstance<FirstClient>();
        GrpcChannel original = container.ContainerScope.GetDisposables().OfType<GrpcChannel>().Single();

        // Act
        Exception? exception = Capture(() => _ = container.RegisterGrpcClient<ISecondContract, SecondClient>(options, (proxy, scribe, timeout) => new(proxy, scribe, timeout)));
        GrpcChannel failed = container.ContainerScope.GetDisposables().OfType<GrpcChannel>().Last();
        Exception? disposed = Capture(() => _ = failed.CreateCallInvoker());

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(ReferenceEquals(original, failed)).IsFalse();
        _ = await Assert.That(disposed).IsTypeOf<ObjectDisposedException>();
        _ = original.CreateCallInvoker();
    }

    [Test]
    public async Task GivenRemoteSettingsThenTheClientUsesTheProxyScribeAndTimeoutWithinEachScope()
    {
        // Arrange
        using Container container = CreateContainer(out IScribe scribe);
        var options = new Remote.GrpcOptions
        {
            Address = new Uri(FirstAddress),
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds),
        };

        // Act
        Container result = container.RegisterGrpcClient<IFirstContract, FirstClient>(options, (proxy, tracing, timeout) => new(proxy, tracing, timeout));
        FirstClient first;
        FirstClient repeated;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            first = container.GetInstance<FirstClient>();
            repeated = container.GetInstance<FirstClient>();
        }

        using DependencyScope next = AsyncScopedLifestyle.BeginScope(container);
        FirstClient second = container.GetInstance<FirstClient>();
        IFirstContract contract = container.GetInstance<IFirstContract>();
        GrpcChannel channel = container.ContainerScope.GetDisposables().OfType<GrpcChannel>().Single();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(first).IsSameReferenceAs(repeated);
        _ = await Assert.That(ReferenceEquals(first, second)).IsFalse();
        _ = await Assert.That(first.Contract).IsSameReferenceAs(contract);
        _ = await Assert.That(second.Contract).IsSameReferenceAs(contract);
        _ = await Assert.That(first.Scribe).IsSameReferenceAs(scribe);
        _ = await Assert.That(first.Timeout).IsEqualTo(options.Timeout);
        _ = await Assert.That(channel.Target).IsEqualTo(options.Address.Authority);
    }

    [Test]
    public async Task GivenTwoRemoteContractsThenTheirChannelsAreIndependentAndOwnedByTheContainer()
    {
        // Arrange
        using Container container = CreateContainer(out _);
        var first = new Remote.GrpcOptions { Address = new Uri(FirstAddress) };
        var second = new Remote.GrpcOptions { Address = new Uri(SecondAddress) };

        // Act
        _ = container.RegisterGrpcClient<IFirstContract, FirstClient>(first, (proxy, scribe, timeout) => new(proxy, scribe, timeout));
        _ = container.RegisterGrpcClient<ISecondContract, SecondClient>(second, (proxy, scribe, timeout) => new(proxy, scribe, timeout));
        GrpcChannel[] channels = container.ContainerScope.GetDisposables().OfType<GrpcChannel>().ToArray();
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        FirstClient firstClient = container.GetInstance<FirstClient>();
        SecondClient secondClient = container.GetInstance<SecondClient>();

        // Assert
        _ = await Assert.That(channels.Length).IsEqualTo(2);
        _ = await Assert.That(channels.Select(channel => channel.Target)).Contains(first.Address.Authority);
        _ = await Assert.That(channels.Select(channel => channel.Target)).Contains(second.Address.Authority);
        _ = await Assert.That(firstClient.Contract).IsSameReferenceAs(container.GetInstance<IFirstContract>());
        _ = await Assert.That(secondClient.Contract).IsSameReferenceAs(container.GetInstance<ISecondContract>());
    }

    [Test]
    [Arguments(SmallMessageLimit, default)]
    [Arguments(default, SmallMessageLimit)]
    public async Task GivenRemoteMessageLimitsWhenAMessageExceedsTheConfiguredLimitThenTheCallIsRejected(int? receiveLimit, int? sendLimit)
    {
        // Arrange
        const int dynamicPort = 0;
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.Services.AddMu(out Container serverContainer);
        using (serverContainer)
        {
            _ = serverContainer.RegisterGrpcHost<EchoService>(new Hosted.GrpcOptions { Port = dynamicPort });
            serverContainer.Register<EchoService>(Lifestyle.Scoped);
            _ = builder.ConfigureMu();
            await using WebApplication host = builder.Build();
            _ = host.UseMu(serverContainer);
            _ = host.MapGrpcService<EchoService>();
            await host.StartAsync();
            IServer server = host.Services.GetRequiredService<IServer>();
            string address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            var endpoint = new UriBuilder(address) { Host = HostName };
            using Container clientContainer = CreateContainer(out _);
            var options = new Remote.GrpcOptions
            {
                Address = endpoint.Uri,
                MaxReceiveMessageSize = receiveLimit,
                MaxSendMessageSize = sendLimit,
            };
            _ = clientContainer.RegisterGrpcClient<IFirstContract, FirstClient>(options, (proxy, scribe, timeout) => new(proxy, scribe, timeout));
            using DependencyScope scope = AsyncScopedLifestyle.BeginScope(clientContainer);
            FirstClient client = clientContainer.GetInstance<FirstClient>();
            var request = new Payload { Value = PayloadValue };

            // Act
            RpcException? exception = default;

            try
            {
                _ = await client.Contract.Execute(request);
            }
            catch (RpcException failure)
            {
                exception = failure;
            }

            // Assert
            _ = await Assert.That(exception).IsNotNull();
            _ = await Assert.That(exception!.StatusCode).IsEqualTo(StatusCode.ResourceExhausted);
        }
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

    private static Container CreateContainer(out IScribe scribe)
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        scribe = Substitute.For<IScribe>();
        container.RegisterInstance(scribe);

        return container;
    }

    public sealed class EchoService : IFirstContract
    {
        public ValueTask<Payload> Execute(Payload request, CallContext context = default)
        {
            return ValueTask.FromResult(request);
        }
    }

    public sealed record FirstClient(IFirstContract Contract, IScribe Scribe, TimeSpan Timeout);

    [ProtoContract]
    public sealed class Payload
    {
        [ProtoMember(1)]
        public string Value { get; set; } = string.Empty;
    }

    public sealed record SecondClient(ISecondContract Contract, IScribe Scribe, TimeSpan Timeout);
}