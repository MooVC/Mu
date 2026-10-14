namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Mu.Configuration;
using SimpleInjector;

public sealed partial class WhenInitializeIsCalled
{
    private const string GuardUnitName = "VehicleRegistrationGuards";

    private static readonly Lazy<Task<Assembly>> _guardAssembly = new(() => CompileRegistrar(
        CreateModeFeatureSource(GuardUnitName),
        GetModeFeatureNamespace(GuardUnitName)));

    [Test]
    [Arguments("RegisterMode", typeof(Options), false)]
    [Arguments("RegisterMode", typeof(Options), true)]
    [Arguments("RegisterIpcHost", typeof(Hosted), false)]
    [Arguments("RegisterIpcHost", typeof(Hosted), true)]
    [Arguments("RegisterIpcClient", typeof(Remote), false)]
    [Arguments("RegisterIpcClient", typeof(Remote), true)]
    public async Task GivenANullContainerThenItsGuardIdentifiesTheContainerBeforeAnyOtherArgument(string methodName, Type optionsType, bool usesConfiguration)
    {
        // Arrange
        const string expectedMessage = "The container must be provided.";
        const string expectedParameter = "container";
        Assembly assembly = await _guardAssembly.Value;
        Type argumentType = usesConfiguration ? typeof(IConfiguration) : optionsType;
        MethodInfo registration = GetGuardRegistration(assembly, methodName, argumentType);

        // Act
        Exception? failure = CaptureModeFailure(() => _ = registration.Invoke(default, [default, default]));

        // Assert
        _ = await Assert.That(failure).IsTypeOf<ArgumentNullException>();
        var exception = (ArgumentNullException)failure!;
        _ = await Assert.That(exception.ParamName).IsEqualTo(expectedParameter);
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    [Arguments("RegisterMode")]
    [Arguments("RegisterIpcHost")]
    [Arguments("RegisterIpcClient")]
    public async Task GivenANullConfigurationThenItsGuardIdentifiesTheConfigurationSource(string methodName)
    {
        // Arrange
        const string expectedMessage = "The source for configuration values must be provided.";
        const string expectedParameter = "configuration";
        Assembly assembly = await _guardAssembly.Value;
        MethodInfo registration = GetGuardRegistration(assembly, methodName, typeof(IConfiguration));
        using var container = new Container();

        // Act
        Exception? failure = CaptureModeFailure(() => _ = registration.Invoke(default, [container, default]));

        // Assert
        _ = await Assert.That(failure).IsTypeOf<ArgumentNullException>();
        var exception = (ArgumentNullException)failure!;
        _ = await Assert.That(exception.ParamName).IsEqualTo(expectedParameter);
        _ = await Assert.That(exception.Message.StartsWith(expectedMessage, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    [Arguments("RegisterMode", typeof(Options))]
    [Arguments("RegisterIpcHost", typeof(Hosted))]
    [Arguments("RegisterIpcClient", typeof(Remote))]
    public async Task GivenNullOptionalOptionsThenRegistrationUsesDefaultsAndReturnsTheContainer(string methodName, Type optionsType)
    {
        // Arrange
        Assembly assembly = await _guardAssembly.Value;
        MethodInfo registration = GetGuardRegistration(assembly, methodName, optionsType);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{GuardUnitName}.{GuardUnitName}", throwOnError: true)!;
        using Container container = CreateModeContainer(aggregate);

        // Act
        object? result = registration.Invoke(default, [container, default]);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
    }

    private static MethodInfo GetGuardRegistration(Assembly assembly, string methodName, Type optionsType)
    {
        string featureNamespace = GetModeFeatureNamespace(GuardUnitName);
        Type extensions = assembly.GetType($"{featureNamespace}.ContainerExtensions", throwOnError: true)!;

        return extensions.GetMethod(methodName, [typeof(Container), optionsType])!;
    }
}