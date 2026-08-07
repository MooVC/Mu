namespace Mu.Sample;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Grpc.AspNetCore.Server;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Mu.Composition;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Sample.Open;
using ProtoBuf.Grpc.Client;
using SimpleInjector;
using AccountAggregate = global::Mu.Sample.Account.Account;
using OpenAccount = global::Mu.Sample.Open.Open;

internal static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(arguments);

        _ = builder.Services.AddMu(out Container container);

        RegisterApplication(container);

        _ = builder.Services.Replace(ServiceDescriptor.Singleton(
            typeof(IGrpcServiceActivator<>),
            typeof(Mu.Composition.gRpc.Activator<>)));

        using WebApplication host = builder
            .ConfigureMu()
            .Build();

        _ = host.MapGrpcService<GrpcService>();
        _ = host.UseMu(container);

        try
        {
            ConfiguredTaskAwaitable task = host
                .RunAsync()
                .ConfigureAwait(false);

            await Task.Delay(1000);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");

            IGrpcService client = channel.CreateGrpcService<IGrpcService>();
            OpenAccount.Result reply = await client.Open(new OpenAccount(new Account.Owner("Alice")));

            Console.WriteLine(reply);

            await task;

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

            return -1;
        }
        finally
        {
            await host
                .StopAsync()
                .ConfigureAwait(false);
        }
    }

    private static void RegisterApplication(Container container)
    {
        container.Collection.Register(Enumerable.Empty<IInvariant<AccountAggregate, OpenAccount>>());
        container.Collection.Append<ITransform<AccountAggregate, Opened>, Transform>();
        container.Register<IRoot<AccountAggregate, OpenAccount>, Root>();
        container.Register<IWriteStore<AccountAggregate, Guid>, WriteStore<AccountAggregate, Guid>>();
        container.Register<ITransform<AccountAggregate>, ReflectionTransform<AccountAggregate>>();
        container.Register<IStream<Guid>, InMemoryStream<Guid>>();
        container.Register<IService<OpenAccount, OpenAccount.Result>, Service>();
        container.RegisterInstance<IServiceProvider>(container);
        container.Register<GrpcService>(Lifestyle.Scoped);
    }
}