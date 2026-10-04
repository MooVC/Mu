namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Mu.Auditing;
using Mu.Communications.Ipc.Grpc;
using Mu.Communications.Mediation;
using Mu.Communications.Tracing;
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

            container.RegisterInstance<IServiceProvider>(container);
            container.Register<IMediator, InMemoryMediator>(Lifestyle.Scoped);
            container.RegisterSingleton<IScopeManager, ScopeManager>();
            container.RegisterSingleton<IScribe, Scribe>();

            _ = services
                .AddLogging()
                .AddSimpleInjector(container, options)
                .AddCodeFirstGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());

            return services;
        }
    }
}