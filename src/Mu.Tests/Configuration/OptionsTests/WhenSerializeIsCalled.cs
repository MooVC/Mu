namespace Mu.Configuration.OptionsTests;

using System.Text.Json;

public sealed class WhenSerializeIsCalled
{
    [Test]
    public async Task GivenDisabledMessageLimitsWhenDeserializedThenHostedAndRemoteLimitsRemainNull()
    {
        // Arrange
        var defaults = new Options();
        Options options = defaults with
        {
            Hosted = defaults.Hosted with
            {
                Grpc = defaults.Hosted.Grpc with
                {
                    MaxReceiveMessageSize = null,
                    MaxSendMessageSize = null,
                },
            },
            Remote = defaults.Remote with
            {
                Grpc = defaults.Remote.Grpc with
                {
                    MaxReceiveMessageSize = null,
                    MaxSendMessageSize = null,
                },
            },
        };

        // Act
        string serialized = JsonSerializer.Serialize(options);
        Options result = JsonSerializer.Deserialize<Options>(serialized)!;

        // Assert
        _ = await Assert.That(result).IsNotNull();
        _ = await Assert.That(result.Hosted.Grpc.MaxReceiveMessageSize).IsNull();
        _ = await Assert.That(result.Hosted.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(result.Remote.Grpc.MaxReceiveMessageSize).IsNull();
        _ = await Assert.That(result.Remote.Grpc.MaxSendMessageSize).IsNull();
        _ = await Assert.That(defaults.Hosted.Grpc.MaxReceiveMessageSize).IsNotNull();
        _ = await Assert.That(defaults.Remote.Grpc.MaxReceiveMessageSize).IsNotNull();
    }
}