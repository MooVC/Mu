namespace Mu.Composition.gRpc;

using System;
using Grpc.AspNetCore.Server;
using SimpleInjector;

public sealed class Activator<TService>(Container container)
    : IGrpcServiceActivator<TService>
    where TService : class
{
    public GrpcActivatorHandle<TService> Create(IServiceProvider serviceProvider)
    {
        TService instance = container.GetInstance<TService>();

        return new GrpcActivatorHandle<TService>(instance, created: false, state: default);
    }

    public ValueTask ReleaseAsync(GrpcActivatorHandle<TService> service)
    {
        return ValueTask.CompletedTask;
    }
}