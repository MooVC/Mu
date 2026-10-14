namespace Mu.Composition;

using Ardalis.GuardClauses;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mu.Auditing;
using Mu.Auditing.Composition;
using Mu.Communications.Ipc.Grpc;
using Mu.Communications.Ipc.Grpc.Composition;
using Mu.Communications.Mediation;
using Mu.Communications.Tracing;
using Mu.Communications.Tracing.Composition;
using Mu.Serialization;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using SimpleInjector.Lifestyles;
using static Mu.Composition.IServiceCollectionExtensions_Resources;

/// <summary>
/// Provides extensions for composing Mu applications.
/// </summary>
public static class IServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the Mu composition root and shared application services to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The configured service collection.</returns>
        public IServiceCollection AddMu()
        {
            return services.AddMu(out _);
        }

        /// <summary>
        /// Adds the Mu composition root and shared application services to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="container">The configured dependency injection container.</param>
        /// <param name="options">Optional configuration for the Simple Injector integration.</param>
        /// <returns>The configured service collection.</returns>
        public IServiceCollection AddMu(out Container container, Action<SimpleInjectorAddOptions>? options = default)
        {
            _ = Guard.Against.Null(services, message: AddMuServicesRequired);

            _ = RuntimeTypeModel.Default.AddMu();

            container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var composition = new CompositionContext(container);
            container.ContainerScope.SetItem(typeof(CompositionContext), composition);
            _ = services.AddSingleton(composition);

            GrpcHostConfiguration configuration = CompositionStartup.Configure(services);
            container.ContainerScope.SetItem(typeof(IServiceCollection), services);
            container.ContainerScope.SetItem(typeof(GrpcHostConfiguration), configuration);

            container
                .RegisterScopeManager()
                .RegisterScribe()
                .RegisterInstance<IServiceProvider>(container);

            _ = services
                .AddLogging()
                .AddSimpleInjector(container, integration =>
                {
                    composition.IntegrationOptions = integration;
                    options?.Invoke(integration);
                })
                .AddCodeFirstGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());

            _ = services.Replace(ServiceDescriptor.Singleton(typeof(IGrpcServiceActivator<>), typeof(ServiceActivator<>)));

            return services;
        }
    }
}