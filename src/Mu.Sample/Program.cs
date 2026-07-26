namespace Mu.Sample;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Mediation;
using Mu.Composition;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Sample.Account;
using Mu.Sample.Open;
using ProtoBuf.Grpc.Server;
using SimpleInjector;
using AccountAggregate = global::Mu.Sample.Account.Account;
using OpenAccount = global::Mu.Sample.Open.Open;

internal static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(arguments);
        _ = builder.WebHost.ConfigureKestrel(options =>
            options.ConfigureEndpointDefaults(endpoint => endpoint.Protocols = HttpProtocols.Http2));
        builder.Services.AddCodeFirstGrpc();

        _ = builder.Services.AddMu(out Container container);
        _ = builder.Services.AddScoped<IMediator, InMemoryMediator>();
        _ = builder.Services.AddScoped<IService<OpenAccount, Guid>>(_ => container.GetInstance<IService<OpenAccount, Guid>>());
        _ = builder.Services.AddScoped<IHandler<OpenAccount, Guid>, ServiceHandler<OpenAccount, Guid>>();

        RegisterApplication(container);

        await using WebApplication application = builder.Build();

        _ = application.MapGrpcService<OpenGrpcService>();
        _ = application.UseMu(container);

        await application
            .RunAsync()
            .ConfigureAwait(false);

        return 0;
    }

    private static void RegisterApplication(Container container)
    {
        container.Collection.Register(Enumerable.Empty<IInvariant<AccountAggregate, OpenAccount>>());
        container.Collection.Append<ITransform<AccountAggregate, Opened>, Transform>();
        container.Register<IRoot<AccountAggregate, OpenAccount>, Root>();
        container.Register<IWriteStore<AccountAggregate, Guid>, Store>(Lifestyle.Singleton);
        container.Register<IService<OpenAccount, Guid>, Service>();
    }
}