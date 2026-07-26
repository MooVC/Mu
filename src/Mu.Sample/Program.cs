namespace Mu.Sample;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mu.Composition;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Persistence;
using Mu.Sample.Account;
using Mu.Sample.Open;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using AccountAggregate = global::Mu.Sample.Account.Account;
using OpenAccount = global::Mu.Sample.Open.Open;

internal static class Program
{
    public static async Task<int> Main(string[] arguments)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(arguments);
        _ = builder.Services.AddMu(out Container container);

        using IHost host = builder
            .Build()
            .UseMu(container);

        RegisterApplication(container);
        container.Verify();
        await host.StartAsync().ConfigureAwait(false);

        try
        {
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                IService<OpenAccount, Guid> service = container.GetInstance<IService<OpenAccount, Guid>>();
                IHostApplicationLifetime lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                string ownerName = builder.Configuration[nameof(Owner)] ?? nameof(Owner);

                Result<Guid> result = await service
                    .Execute(new(new(ownerName)), lifetime.ApplicationStopping)
                    .ConfigureAwait(false);

                if (result.IsSuccessful)
                {
                    Console.WriteLine(result.Value);

                    return 0;
                }

                foreach (ValidationResult failure in result.Failures)
                {
                    await Console.Error.WriteLineAsync(failure.ErrorMessage).ConfigureAwait(false);
                }

                return 1;
            }
        }
        finally
        {
            await host.StopAsync().ConfigureAwait(false);
        }
    }

    private static void RegisterApplication(Container container)
    {
        container.Collection.Register(Enumerable.Empty<IInvariant<AccountAggregate, OpenAccount>>());
        container.Collection.Append<ITransform<AccountAggregate, Opened>, Transform>();
        container.Register<IRoot<AccountAggregate, OpenAccount>, Root>();
        container.Register<IWriteStore<AccountAggregate, Guid>, WriteStore<AccountAggregate, Guid>>();
        container.Register<IService<OpenAccount, Guid>, Service>();
    }
}