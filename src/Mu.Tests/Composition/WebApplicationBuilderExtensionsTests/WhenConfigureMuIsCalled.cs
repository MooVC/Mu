namespace Mu.Composition.WebApplicationBuilderExtensionsTests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

public sealed class WhenConfigureMuIsCalled
{
    private const int DefaultPort = 50051;

    private const int DynamicPort = 0;

    [Test]
    public async Task GivenNoMuServicesThenTheDefaultListenerIsStarted()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.ConfigureMu();
        await using WebApplication host = builder.Build();

        // Act
        await host.StartAsync();
        IServer server = host.Services.GetRequiredService<IServer>();
        ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;

        // Assert
        string address = await Assert.That(addresses).HasSingleItem();
        _ = await Assert.That(new Uri(address).Port).IsEqualTo(DefaultPort);
    }

    [Test]
    public async Task GivenAnExplicitListenerAfterDefaultConfigurationThenOnlyTheExplicitListenerIsStarted()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.ConfigureMu();
        _ = builder.ConfigureMu(options => options.ListenAnyIP(DynamicPort, listener => listener.Protocols = HttpProtocols.Http2));
        await using WebApplication host = builder.Build();

        // Act
        await host.StartAsync();
        IServer server = host.Services.GetRequiredService<IServer>();
        ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;

        // Assert
        string address = await Assert.That(addresses).HasSingleItem();
        _ = await Assert.That(new Uri(address).Port).IsGreaterThan(DynamicPort);
    }
}