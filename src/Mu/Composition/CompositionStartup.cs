namespace Mu.Composition;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mu.Communications.Ipc.Grpc.Composition;

internal sealed class CompositionStartup(GrpcHostConfiguration configuration, IServiceProvider services) : IHostedLifecycleService
{
    private const int DefaultPort = 50051;

    public static GrpcHostConfiguration Configure(IServiceCollection services)
    {
        GrpcHostConfiguration? configuration = services
            .Select(service => service.ImplementationInstance)
            .OfType<GrpcHostConfiguration>()
            .FirstOrDefault();

        if (configuration is null)
        {
            configuration = new GrpcHostConfiguration();
            _ = services.AddSingleton(configuration);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, CompositionStartup>());

        return configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (configuration.Ports.Count > 0 || configuration.UseDefaultPort)
        {
            KestrelServerOptions options = services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;
            IEnumerable<int> ports = configuration.Ports.Count > 0 ? configuration.Ports : [DefaultPort];

            foreach (int port in ports)
            {
                options.ListenAnyIP(port, listener => listener.Protocols = HttpProtocols.Http2);
            }
        }

        if (services.GetService<CompositionContext>() is { VerifyOnStartup: true } composition)
        {
            composition.Container.Verify();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}