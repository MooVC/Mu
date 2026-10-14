namespace Mu.Communications.Ipc.Grpc.Composition;

internal sealed class GrpcHostConfiguration
{
    public HashSet<int> Ports { get; } = [];

    public bool UseDefaultPort { get; set; }
}