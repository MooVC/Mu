namespace Mu.Configuration.OptionsTests;

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Mu.Testing;

public sealed class WhenGetIsCalled
{
    [Test]
    public async Task GivenFeatureSettingsThenInheritedHostedAndRemoteGrpcSettingsAreBound()
    {
        // Arrange
        const string expectedAddress = "https://example.test:7443";
        const string expectedDescription = "Feature configuration";
        const int expectedHostedReceiveLimit = 8 * 1024 * 1024;
        const int expectedHostedSendLimit = 2 * 1024 * 1024;
        const int expectedPort = 60051;
        const int expectedRemoteReceiveLimit = 12 * 1024 * 1024;
        const int expectedRemoteSendLimit = 3 * 1024 * 1024;
        const int expectedTimeoutSeconds = 45;
        const string timeout = "00:00:45";
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(TestFeatureOptions)}:{nameof(TestFeatureOptions.Description)}"] = expectedDescription,
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Mode)}"] = nameof(ModeType.Remote),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Hosted)}:{nameof(Hosted.Ipc)}"] = nameof(IpcType.None),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.EnableDetailedErrors)}"] = bool.TrueString,
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxReceiveMessageSize)}"] = expectedHostedReceiveLimit.ToString(CultureInfo.InvariantCulture),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxSendMessageSize)}"] = expectedHostedSendLimit.ToString(CultureInfo.InvariantCulture),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.Port)}"] = expectedPort.ToString(CultureInfo.InvariantCulture),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Remote)}:{nameof(Remote.Ipc)}"] = nameof(IpcType.Grpc),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Remote)}:{nameof(Remote.Grpc)}:{nameof(Remote.GrpcOptions.Address)}"] = expectedAddress,
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Remote)}:{nameof(Remote.Grpc)}:{nameof(Remote.GrpcOptions.MaxReceiveMessageSize)}"] = expectedRemoteReceiveLimit.ToString(CultureInfo.InvariantCulture),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Remote)}:{nameof(Remote.Grpc)}:{nameof(Remote.GrpcOptions.MaxSendMessageSize)}"] = expectedRemoteSendLimit.ToString(CultureInfo.InvariantCulture),
            [$"{nameof(TestFeatureOptions)}:{nameof(Options.Remote)}:{nameof(Remote.Grpc)}:{nameof(Remote.GrpcOptions.Timeout)}"] = timeout,
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        TestFeatureOptions result = configuration.GetSection(nameof(TestFeatureOptions)).Get<TestFeatureOptions>()!;

        // Assert
        _ = await Assert.That(result).IsNotNull();
        _ = await Assert.That(result.Description).IsEqualTo(expectedDescription);
        _ = await Assert.That(result.Mode).IsEqualTo(ModeType.Remote);
        _ = await Assert.That(result.Hosted.Ipc).IsEqualTo(IpcType.None);
        _ = await Assert.That(result.Hosted.Grpc).IsTypeOf<Hosted.GrpcOptions>();
        _ = await Assert.That(result.Hosted.Grpc.EnableDetailedErrors).IsTrue();
        _ = await Assert.That(result.Hosted.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedHostedReceiveLimit);
        _ = await Assert.That(result.Hosted.Grpc.MaxSendMessageSize).IsEqualTo(expectedHostedSendLimit);
        _ = await Assert.That(result.Hosted.Grpc.Port).IsEqualTo(expectedPort);
        _ = await Assert.That(result.Remote.Ipc).IsEqualTo(IpcType.Grpc);
        _ = await Assert.That(result.Remote.Grpc).IsTypeOf<Remote.GrpcOptions>();
        _ = await Assert.That(result.Remote.Grpc.Address).IsEqualTo(new Uri(expectedAddress));
        _ = await Assert.That(result.Remote.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedRemoteReceiveLimit);
        _ = await Assert.That(result.Remote.Grpc.MaxSendMessageSize).IsEqualTo(expectedRemoteSendLimit);
        _ = await Assert.That(result.Remote.Grpc.Timeout).IsEqualTo(TimeSpan.FromSeconds(expectedTimeoutSeconds));
    }

    [Test]
    public async Task GivenOnlyRemoteModeThenUnspecifiedGrpcSettingsRetainTheirDefaults()
    {
        // Arrange
        const string expectedAddress = "http://localhost:50051";
        const int expectedReceiveLimit = 4 * 1024 * 1024;
        const int expectedPort = 50051;
        const int expectedTimeoutSeconds = 30;
        var values = new Dictionary<string, string?>
        {
            [nameof(Options.Mode)] = nameof(ModeType.Remote),
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        Options result = configuration.Get<Options>()!;

        // Assert
        _ = await Assert.That(result).IsNotNull();
        _ = await Assert.That(result.Mode).IsEqualTo(ModeType.Remote);
        _ = await Assert.That(result.Hosted.Grpc.EnableDetailedErrors).IsFalse();
        _ = await Assert.That(result.Hosted.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedReceiveLimit);
        _ = await Assert.That(result.Hosted.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(result.Hosted.Grpc.Port).IsEqualTo(expectedPort);
        _ = await Assert.That(result.Remote.Grpc.Address).IsEqualTo(new Uri(expectedAddress));
        _ = await Assert.That(result.Remote.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedReceiveLimit);
        _ = await Assert.That(result.Remote.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(result.Remote.Grpc.Timeout).IsEqualTo(TimeSpan.FromSeconds(expectedTimeoutSeconds));
    }
}