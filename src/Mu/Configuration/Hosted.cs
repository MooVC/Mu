namespace Mu.Configuration;

public sealed partial record Hosted
{
    public IpcType Ipc { get; init; } = IpcType.Grpc;

    public GrpcOptions Grpc { get; init; } = new();
}