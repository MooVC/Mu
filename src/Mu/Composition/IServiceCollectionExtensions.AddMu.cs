namespace Mu.Composition;

using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Ipc.Grpc;
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
        /// Adds the Mu composition root to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The configured dependency injection container.</returns>
        public IServiceCollection AddMu()
        {
            return services.AddMu(out _);
        }

        /// <summary>
        /// Adds the Mu composition root to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="container">The configured dependency injection container.</param>
        /// <returns>The configured dependency injection container.</returns>
        public IServiceCollection AddMu(out Container container, Action<SimpleInjectorAddOptions>? options = default)
        {
            _ = Guard.Against.Null(services, message: AddMuServicesRequired);

            _ = RuntimeTypeModel.Default.AddMu();

            container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            options ??= options => options.AddLogging();

            _ = services
                .AddSimpleInjector(container, options)
                .AddCodeFirstGrpc(options => options.Interceptors.Add<ExceptionInterceptor>());

            return services;
        }
    }
}