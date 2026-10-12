namespace Mu.Configuration.OptionsTests;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenNoSettingsThenHostedAndRemoteGrpcDefaultsAreAvailable()
    {
        // Arrange
        const string expectedAddress = "http://localhost:50051";
        const int expectedReceiveLimit = 4 * 1024 * 1024;
        const int expectedPort = 50051;
        const int expectedTimeoutSeconds = 30;

        // Act
        var result = new Options();

        // Assert
        _ = await Assert.That(result.Mode).IsEqualTo(ModeType.Hosted);
        _ = await Assert.That(result.Hosted.Ipc).IsEqualTo(IpcType.Grpc);
        _ = await Assert.That(result.Hosted.Grpc).IsTypeOf<Hosted.GrpcOptions>();
        _ = await Assert.That(result.Hosted.Grpc.EnableDetailedErrors).IsFalse();
        _ = await Assert.That(result.Hosted.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedReceiveLimit);
        _ = await Assert.That(result.Hosted.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(result.Hosted.Grpc.Port).IsEqualTo(expectedPort);
        _ = await Assert.That(result.Remote.Ipc).IsEqualTo(IpcType.Grpc);
        _ = await Assert.That(result.Remote.Grpc).IsTypeOf<Remote.GrpcOptions>();
        _ = await Assert.That(result.Remote.Grpc.Address).IsEqualTo(new Uri(expectedAddress));
        _ = await Assert.That(result.Remote.Grpc.MaxReceiveMessageSize).IsEqualTo(expectedReceiveLimit);
        _ = await Assert.That(result.Remote.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(result.Remote.Grpc.Timeout).IsEqualTo(TimeSpan.FromSeconds(expectedTimeoutSeconds));
    }
}