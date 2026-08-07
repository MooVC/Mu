namespace Mu.Composition.IServiceCollectionExtensionsTests;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Meta;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed class WhenAddMuIsCalled
{
    private const int TimestampOffsetMinutes = 345;

    [Test]
    public async Task GivenACompositionRootThenSimpleInjectorIsConfigured()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();

        // Act
        _ = root.Services.AddMu(out Container container);
        container.Register<Subject>();

        using IHost host = root
            .Build()
            .UseSimpleInjector(container);

        container.Verify();
        Subject subject = container.GetInstance<Subject>();

        // Assert
        _ = await Assert.That(container.Options.DefaultScopedLifestyle).IsTypeOf<AsyncScopedLifestyle>();
        _ = await Assert.That(subject.Logger).IsNotNull();
    }

    [Test]
    public async Task GivenACompositionRootThenDateTimeOffsetSerializationIsConfigured()
    {
        // Arrange
        HostApplicationBuilder root = Host.CreateApplicationBuilder();
        var offset = TimeSpan.FromMinutes(TimestampOffsetMinutes);
        var subject = new DateTimeOffset(2024, 5, 6, 7, 8, 9, offset);

        // Act
        _ = root.Services.AddMu();
        _ = root.Services.AddMu();

        DateTimeOffset result = RuntimeTypeModel.Default.DeepClone(subject);

        // Assert
        _ = await Assert.That(result).IsEqualTo(subject);
        _ = await Assert.That(result.Offset).IsEqualTo(subject.Offset);
    }

    private sealed class Subject(ILogger logger)
    {
        public ILogger Logger { get; } = logger;
    }
}