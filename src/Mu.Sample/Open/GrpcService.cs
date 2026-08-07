namespace Mu.Sample.Open;

using System;
using System.ComponentModel;
using Mu;
using Mu.Modelling.Services;
using ProtoBuf.Grpc;

public sealed class GrpcService(IService<Open, Open.Result> service)
    : IGrpcService
{
    public async ValueTask<Open.Result> Open(Open open, CallContext context = default)
    {
        Result<Open.Result> id = await service
            .Execute(open, context.CancellationToken)
            .ConfigureAwait(false);

        return id.Value!;
    }
}