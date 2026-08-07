namespace Mu.Sample.Open;

using System;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;

[Service]
public interface IGrpcService
{
    ValueTask<Open.Result> Open(Open open, CallContext context = default);
}