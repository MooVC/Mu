namespace Mu.Communications.Ipc.Grpc.Composition;

using Ardalis.GuardClauses;
using global::Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mu.Composition;
using Mu.Configuration;
using SimpleInjector;
using static Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions_Resources;
using OptionsFactory = Microsoft.Extensions.Options.Options;

public static partial class ContainerExtensions
{
    extension(Container container)
    {
        /// <summary>
        /// Applies the hosted gRPC settings to the container and its attached application services.
        /// </summary>
        /// <typeparam name="TService">The hosted gRPC service.</typeparam>
        /// <param name="options">The hosted gRPC settings.</param>
        /// <returns>The container.</returns>
        public Container RegisterGrpcHost<TService>(Hosted.GrpcOptions options)
            where TService : class
        {
            _ = Guard.Against.Null(container, message: RegisterGrpcHostContainerRequired);
            _ = Guard.Against.Null(options, message: RegisterGrpcHostOptionsRequired);

            var settings = new GrpcServiceOptions<TService>
            {
                EnableDetailedErrors = options.EnableDetailedErrors,
                MaxReceiveMessageSize = options.MaxReceiveMessageSize,
                MaxSendMessageSize = options.MaxSendMessageSize,
            };

            container.RegisterInstance<IOptions<GrpcServiceOptions<TService>>>(OptionsFactory.Create(settings));

            if (container.ContainerScope.GetItem(typeof(IServiceCollection)) is IServiceCollection services)
            {
                if (container.ContainerScope.GetItem(typeof(CompositionContext)) is CompositionContext { ServiceProvider: not null } composition)
                {
                    GrpcServiceOptions<TService> grpc = composition.ServiceProvider.GetRequiredService<IOptions<GrpcServiceOptions<TService>>>().Value;
                    grpc.EnableDetailedErrors = options.EnableDetailedErrors;
                    grpc.MaxReceiveMessageSize = options.MaxReceiveMessageSize;
                    grpc.MaxSendMessageSize = options.MaxSendMessageSize;
                }
                else
                {
                    _ = services.AddGrpc().AddServiceOptions<TService>(grpc =>
                    {
                        grpc.EnableDetailedErrors = options.EnableDetailedErrors;
                        grpc.MaxReceiveMessageSize = options.MaxReceiveMessageSize;
                        grpc.MaxSendMessageSize = options.MaxSendMessageSize;
                    });
                }

                var configuration = (GrpcHostConfiguration)container.ContainerScope.GetItem(typeof(GrpcHostConfiguration))!;
                _ = configuration.Ports.Add(options.Port);
            }

            return container;
        }
    }
}