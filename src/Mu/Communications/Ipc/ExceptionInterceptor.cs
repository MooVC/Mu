namespace Mu.Communications.Ipc;

using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

public sealed partial class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context)
                .ConfigureAwait(false);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogUnhandledGrpcError(ex, context.Method);

            throw new RpcException(new Status(StatusCode.Internal, "An internal server error occurred."));
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unhandled Exception Executing gRPC method {Method}")]
    private partial void LogUnhandledGrpcError(Exception exception, string method);
}