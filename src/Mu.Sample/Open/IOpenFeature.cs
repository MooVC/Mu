namespace Mu.Sample.Open;

using ProtoBuf.Grpc.Configuration;

[Service("mu.sample.open.OpenFeature")]
public interface IOpenFeature
{
    [Operation("Execute")]
    Task<OpenResponse> Execute(Open request, CancellationToken cancellationToken = default);
}