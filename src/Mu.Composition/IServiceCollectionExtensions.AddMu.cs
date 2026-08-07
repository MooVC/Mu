namespace Mu.Composition;

using Ardalis.GuardClauses;
using Grpc.AspNetCore.Server;
using Microsoft.Extensions.DependencyInjection;
using Mu.Composition.gRpc;
using Mu.Serialization;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using SimpleInjector.Lifestyles;

/// <summary>
/// Provides extensions for composing Mu applications.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Mu composition root to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured dependency injection container.</returns>
    public static IServiceCollection AddMu(this IServiceCollection services)
    {
        return services.AddMu(out _);
    }

    /// <summary>
    /// Adds the Mu composition root to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="container">The configured dependency injection container.</param>
    /// <returns>The configured dependency injection container.</returns>
    public static IServiceCollection AddMu(this IServiceCollection services, out Container container, Action<SimpleInjectorAddOptions>? options = default)
    {
        _ = Guard.Against.Null(services, message: "The service collection must be provided.");

        _ = RuntimeTypeModel.Default.AddMu();

        container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

        options ??= options => options.AddLogging();

        services
            .AddSimpleInjector(container, options)
            .AddCodeFirstGrpc();

        return services;
    }
}