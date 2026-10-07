namespace Mu.Auditing.Composition.ContainerExtensionsTests;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Mu.Auditing.Configuration;
using SimpleInjector;

public sealed class WhenRegisterAuditorIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenMissingOptionsThenRegistersDefaultSingletonAndReturnsContainer(bool explicitlyNull)
    {
        // Arrange
        using var subject = new Container();

        // Act
        Container result = explicitlyNull
            ? subject.RegisterAuditor(options: default)
            : subject.RegisterAuditor();
        IAuditor first = subject.GetInstance<IAuditor>();
        IAuditor second = subject.GetInstance<IAuditor>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenInMemoryOptionsThenRegistersSingletonAndReturnsContainer()
    {
        // Arrange
        using var subject = new Container();
        var options = new AuditOptions { Type = AuditorType.InMemory };

        // Act
        Container result = subject.RegisterAuditor(options);
        IAuditor first = subject.GetInstance<IAuditor>();
        IAuditor second = subject.GetInstance<IAuditor>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenUndefinedAuditorTypeThenRegistersDefaultAuditor()
    {
        // Arrange
        const int undefinedType = int.MaxValue;
        using var subject = new Container();
        var options = new AuditOptions { Type = (AuditorType)undefinedType };

        // Act
        _ = subject.RegisterAuditor(options);
        IAuditor result = subject.GetInstance<IAuditor>();

        // Assert
        _ = await Assert.That(result).IsTypeOf<InMemoryAuditor>();
    }

    [Test]
    public async Task GivenEmptyConfigurationThenRegistersDefaultAuditorAndReturnsContainer()
    {
        // Arrange
        using var subject = new Container();
        IConfiguration configuration = new ConfigurationBuilder().Build();

        // Act
        Container result = subject.RegisterAuditor(configuration);
        IAuditor auditor = subject.GetInstance<IAuditor>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(auditor).IsTypeOf<InMemoryAuditor>();
    }

    [Test]
    public async Task GivenConfiguredAuditorThenRegistersSingletonAndReturnsContainer()
    {
        // Arrange
        using var subject = new Container();
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(AuditOptions)}:{nameof(AuditOptions.Type)}"] = nameof(AuditorType.InMemory),
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        Container result = subject.RegisterAuditor(configuration);
        IAuditor first = subject.GetInstance<IAuditor>();
        IAuditor second = subject.GetInstance<IAuditor>();

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(subject);
        _ = await Assert.That(first).IsTypeOf<InMemoryAuditor>();
        _ = await Assert.That(second).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GivenInvalidConfiguredTypeThenThrowsInvalidOperationException()
    {
        // Arrange
        const string invalidType = "UnknownAuditor";
        using var subject = new Container();
        var values = new Dictionary<string, string?>
        {
            [$"{nameof(AuditOptions)}:{nameof(AuditOptions.Type)}"] = invalidType,
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        // Act
        Exception? exception = Capture(() => _ = subject.RegisterAuditor(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<InvalidOperationException>();
    }

    [Test]
    public async Task GivenNullConfigurationThenThrowsArgumentNullException()
    {
        // Arrange
        const string expectedMessage = "The source for configuration values must be provided.";
        IConfiguration configuration = null!;
        using var subject = new Container();

        // Act
        Exception? exception = Capture(() => _ = subject.RegisterAuditor(configuration));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        _ = await Assert.That(((ArgumentNullException)exception!).ParamName).IsEqualTo(nameof(configuration));
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}