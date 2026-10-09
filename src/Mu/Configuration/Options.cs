namespace Mu.Configuration;

public record Options
{
    public IpcType Ipc { get; init; } = IpcType.Grpc;
}