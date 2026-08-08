namespace Mu.Sample;

using System.Runtime.CompilerServices;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Mu.Composition;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Sample.Open;
using ProtoBuf.Grpc.Client;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;
using AccountAggregate = global::Mu.Sample.Account.Account;
using OpenAccount = global::Mu.Sample.Open.Open;

internal static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(arguments);
        using WebApplication host = builder.BuildMu(add: RegisterApplication);

        _ = host.MapGrpcService<GrpcService>();

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

    private static void RegisterApplication(SimpleInjectorAddOptions options)
    {
        options.Container.Collection.Register(Enumerable.Empty<IInvariant<AccountAggregate, OpenAccount>>());
        options.Container.Collection.Append<ITransform<AccountAggregate, Opened>, Transform>();
        options.Container.Register<IRoot<AccountAggregate, OpenAccount>, Root>();
        options.Container.Register<IWriteStore<AccountAggregate, Guid>, WriteStore<AccountAggregate, Guid>>();
        options.Container.Register<ITransform<AccountAggregate>, ReflectionTransform<AccountAggregate>>();
        options.Container.Register<IStream<Guid>, InMemoryStream<Guid>>();
        options.Container.Register<IService<OpenAccount, OpenAccount.Result>, Service>();
        options.Container.RegisterInstance<IServiceProvider>(options.Container);
        options.Container.Register<GrpcService>(Lifestyle.Scoped);
    }
}