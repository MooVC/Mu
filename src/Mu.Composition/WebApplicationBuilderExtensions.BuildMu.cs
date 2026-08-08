namespace Mu.Composition;

using System;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mu.Composition.gRpc;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplication BuildMu(
        this WebApplicationBuilder builder,
        Action<SimpleInjectorAddOptions>? add = default,
        Action<KestrelServerOptions>? kestrel = default,
        Action<SimpleInjectorUseOptions>? use = default)
    {
        return builder.BuildMu(out _, add, kestrel, use);
    }

    public static WebApplication BuildMu(
        this WebApplicationBuilder builder,
        out Container container,
        Action<SimpleInjectorAddOptions>? add = default,
        Action<KestrelServerOptions>? kestrel = default,
        Action<SimpleInjectorUseOptions>? use = default)
    {
        _ = builder.Services.AddMu(out container, options: add);
        _ = builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IGrpcServiceActivator<>), typeof(ServiceActivator<>)));

        WebApplication host = builder
            .ConfigureMu(kestrel: kestrel)
            .Build();

        _ = host.UseMu(container, options: use);

        return host;
    }
}