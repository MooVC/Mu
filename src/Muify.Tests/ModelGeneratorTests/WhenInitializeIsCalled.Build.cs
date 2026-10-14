namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mu.Composition;
using Mu.Configuration;
using Mu.Persistence;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;
using Options = Mu.Configuration.Options;

public sealed partial class WhenInitializeIsCalled
{
    private const string BuildAreaNamespace = "MooVC.Testing.Hosting";

    private const string BuildExtensionType = "WebApplicationExtensions";

    [Test]
    [Arguments(ModeType.Hosted, IpcType.Grpc)]
    [Arguments(ModeType.Hosted, IpcType.None)]
    [Arguments(ModeType.Remote, IpcType.Grpc)]
    [Arguments(ModeType.Remote, IpcType.None)]
    public async Task GivenAnExplicitCustomRegistrarWhenModeAndIpcAreConfiguredThenOnlyHostedGrpcMapsItsEndpoints(ModeType mode, IpcType ipc)
    {
        // Arrange
        const string featureName = "Open";
        const string configurationProperty = "Configuration";
        const string markerName = "RegistrationMarker";
        const string optionsName = "OpenOptions";
        const int expectedRegistrations = 1;
        const int receiveLimit = 1024;
        const int sendLimit = 2048;
        string unitName = $"VehicleBuild{mode}{ipc}";
        string featureNamespace = GetBuildFeatureNamespace(unitName, featureName);
        Assembly assembly = await CompileRegistrar(CreateBuildFeatureSource(unitName, featureName, hasCustomRegistrar: true), featureNamespace);
        Type registrar = assembly.GetType($"{featureNamespace}.{featureName}", throwOnError: true)!;
        Type marker = assembly.GetType($"{featureNamespace}.{markerName}", throwOnError: true)!;
        Type service = assembly.GetType($"{featureNamespace}.{featureName}Service+Grpc+Service", throwOnError: true)!;
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        var settings = new Dictionary<string, string?>
        {
            [$"{optionsName}:{nameof(Options.Mode)}"] = mode.ToString(),
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Ipc)}"] = ipc.ToString(),
            [$"{optionsName}:{nameof(Options.Remote)}:{nameof(Remote.Ipc)}"] = ipc.ToString(),
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.EnableDetailedErrors)}"] = bool.TrueString,
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxReceiveMessageSize)}"] = receiveLimit.ToString(),
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxSendMessageSize)}"] = sendLimit.ToString(),
        };
        _ = builder.Configuration.AddInMemoryCollection(settings);
        await using WebApplication application = builder.BuildMu(out Container container);

        // Act
        WebApplication result = InvokeFeatureBuild(assembly, featureNamespace, featureName, application);
        bool isLockedAfterRegistration = container.IsLocked;
        GrpcMethodMetadata[] methods = GetBuildGrpcMethods(application);
        object registered = container.GetInstance(marker);
        object? configuration = marker.GetProperty(configurationProperty)!.GetValue(registered);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(application);
        _ = await Assert.That(application.GetMuContainer()).IsSameReferenceAs(container);
        _ = await Assert.That(isLockedAfterRegistration).IsFalse();
        _ = await Assert.That((bool)registrar.GetProperty("WasLocked")!.GetValue(default)!).IsFalse();
        _ = await Assert.That((int)registrar.GetProperty("Registrations")!.GetValue(default)!).IsEqualTo(expectedRegistrations);
        _ = await Assert.That(configuration).IsSameReferenceAs(application.Configuration);
        _ = await Assert.That(methods.Any(method => method.ServiceType == service)).IsEqualTo(mode == ModeType.Hosted && ipc == IpcType.Grpc);

        if (mode == ModeType.Hosted && ipc == IpcType.Grpc)
        {
            Type settingsType = typeof(GrpcServiceOptions<>).MakeGenericType(service);
            Type settingsContract = typeof(IOptions<>).MakeGenericType(settingsType);
            object configured = application.Services.GetRequiredService(settingsContract);
            object options = settingsContract.GetProperty(nameof(IOptions<object>.Value))!.GetValue(configured)!;
            _ = await Assert.That((bool?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.EnableDetailedErrors))!.GetValue(options)).IsTrue();
            _ = await Assert.That((int?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.MaxReceiveMessageSize))!.GetValue(options)).IsEqualTo(receiveLimit);
            _ = await Assert.That((int?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.MaxSendMessageSize))!.GetValue(options)).IsEqualTo(sendLimit);
        }
    }

    [Test]
    public async Task GivenTwoGeneratedRegistrarsWhenFeaturesAreChainedAfterCompositionThenStartupVerifiesTheirSharedContainer()
    {
        // Arrange
        const string firstFeature = "Open";
        const string firstOptions = "OpenOptions";
        const string firstUnit = "VehicleBuildChainOpen";
        const string secondFeature = "Start";
        const string secondOptions = "StartOptions";
        const string secondUnit = "VehicleBuildChainStart";
        const string dynamicPort = "0";
        const int fallbackPort = 50051;
        string firstNamespace = GetBuildFeatureNamespace(firstUnit, firstFeature);
        string secondNamespace = GetBuildFeatureNamespace(secondUnit, secondFeature);
        Assembly firstAssembly = await CompileRegistrar(CreateBuildFeatureSource(firstUnit, firstFeature), firstNamespace);
        Assembly secondAssembly = await CompileRegistrar(CreateBuildFeatureSource(secondUnit, secondFeature), secondNamespace);
        Type firstAggregate = firstAssembly.GetType($"{BuildAreaNamespace}.{firstUnit}.{firstUnit}", throwOnError: true)!;
        Type secondAggregate = secondAssembly.GetType($"{BuildAreaNamespace}.{secondUnit}.{secondUnit}", throwOnError: true)!;
        Type firstService = firstAssembly.GetType($"{firstNamespace}.{firstFeature}Service+Grpc+Service", throwOnError: true)!;
        Type secondService = secondAssembly.GetType($"{secondNamespace}.{secondFeature}Service+Grpc+Service", throwOnError: true)!;
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        var settings = new Dictionary<string, string?>
        {
            [$"{firstOptions}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.Port)}"] = dynamicPort,
            [$"{secondOptions}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.Port)}"] = dynamicPort,
        };
        _ = builder.Configuration.AddInMemoryCollection(settings);
        await using WebApplication application = builder.BuildMu(out Container container);
        RegisterBuildWriteStore(container, firstAggregate);
        RegisterBuildWriteStore(container, secondAggregate);
        bool isLockedBeforeFeatures = container.IsLocked;

        // Act
        WebApplication first = InvokeFeatureBuild(firstAssembly, firstNamespace, firstFeature, application);
        WebApplication second = InvokeFeatureBuild(secondAssembly, secondNamespace, secondFeature, first);
        bool isLockedBeforeStartup = container.IsLocked;
        await application.StartAsync();
        bool isLockedAfterStartup = container.IsLocked;
        IServer server = application.Services.GetRequiredService<IServer>();
        ICollection<string> addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;
        object firstInstance;
        object secondInstance;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            firstInstance = container.GetInstance(firstService);
            secondInstance = container.GetInstance(secondService);
        }

        GrpcMethodMetadata[] methods = GetBuildGrpcMethods(application);
        await application.StopAsync();

        // Assert
        _ = await Assert.That(first).IsSameReferenceAs(application);
        _ = await Assert.That(second).IsSameReferenceAs(application);
        _ = await Assert.That(isLockedBeforeFeatures).IsFalse();
        _ = await Assert.That(isLockedBeforeStartup).IsFalse();
        _ = await Assert.That(isLockedAfterStartup).IsTrue();
        _ = await Assert.That(firstInstance.GetType()).IsEqualTo(firstService);
        _ = await Assert.That(secondInstance.GetType()).IsEqualTo(secondService);
        _ = await Assert.That(methods.Any(method => method.ServiceType == firstService)).IsTrue();
        _ = await Assert.That(methods.Any(method => method.ServiceType == secondService)).IsTrue();
        string address = await Assert.That(addresses).HasSingleItem();
        _ = await Assert.That(new Uri(address).Port).IsNotEqualTo(fallbackPort);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenANullOrUncomposedApplicationThenTheRequiredCompositionGuardThrows(bool isNull)
    {
        // Arrange
        const string featureName = "Open";
        const string applicationParameter = "app";
        const string applicationMessage = "The application must be provided.";
        const string compositionMessage = "Mu must be added before building features.";
        string unitName = isNull ? "VehicleBuildNull" : "VehicleBuildUncomposed";
        string featureNamespace = GetBuildFeatureNamespace(unitName, featureName);
        Assembly assembly = await CompileRegistrar(CreateBuildFeatureSource(unitName, featureName), featureNamespace);
        await using WebApplication application = WebApplication.CreateBuilder().Build();

        // Act
        Exception? failure = CaptureBuildFailure(() => _ = InvokeFeatureBuild(assembly, featureNamespace, featureName, isNull ? default! : application));

        // Assert
        if (isNull)
        {
            _ = await Assert.That(failure).IsTypeOf<ArgumentNullException>();
            _ = await Assert.That(((ArgumentNullException)failure!).ParamName).IsEqualTo(applicationParameter);
            _ = await Assert.That(failure.Message.StartsWith(applicationMessage, StringComparison.Ordinal)).IsTrue();
        }
        else
        {
            _ = await Assert.That(failure).IsTypeOf<InvalidOperationException>();
            _ = await Assert.That(failure!.Message).IsEqualTo(compositionMessage);
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAnExistingBuildMethodWhenItIsPublicOrPrivateThenOnlyThePublicMethodSuppressesGeneration(bool isPublic)
    {
        // Arrange
        const string featureName = "Open";
        string unitName = isPublic ? "VehicleBuildPublic" : "VehicleBuildPrivate";
        string featureNamespace = GetBuildFeatureNamespace(unitName, featureName);
        string visibility = isPublic ? "public" : "private";
        string source = CreateBuildFeatureSource(unitName, featureName) + $$"""

            namespace {{featureNamespace}}
            {
                public static partial class WebApplicationExtensions
                {
                    {{visibility}} static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(
                        this global::Microsoft.AspNetCore.Builder.WebApplication app,
                        bool custom = default)
                    {
                        return app;
                    }
                }
            }
            """;
        CSharpCompilation compilation = CreateBuildCompilation(source, featureNamespace);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Any(definition => definition.HintName == $"{featureNamespace}.{BuildExtensionType}.Build{featureName}.g.cs")).IsEqualTo(!isPublic);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAGeneratedCSharpBuildExtensionWhenGenerationIsRepeatedThenItsMethodIsNotDuplicated()
    {
        // Arrange
        const string featureName = "Open";
        const string unitName = "VehicleBuildRepeated";
        string featureNamespace = GetBuildFeatureNamespace(unitName, featureName);
        string expectedHint = $"{featureNamespace}.{BuildExtensionType}.Build{featureName}.g.cs";
        CSharpCompilation compilation = CreateBuildCompilation(CreateBuildFeatureSource(unitName, featureName), featureNamespace);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult initial = driver.GetRunResult().Results.Single();

        // Act
        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(initial.Diagnostics).IsEmpty();
        _ = await Assert.That(initial.GeneratedSources.Any(definition => definition.HintName == expectedHint)).IsTrue();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources.Any(definition => definition.HintName == expectedHint)).IsFalse();
    }

    private static Exception? CaptureBuildFailure(Action action)
    {
        try
        {
            action();
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException;
        }

        return default;
    }

    private static CSharpCompilation CreateBuildCompilation(string source, string assemblyName)
    {
        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static string CreateBuildFeatureSource(string unitName, string featureName, bool hasCustomRegistrar = false)
    {
        string unitNamespace = $"{BuildAreaNamespace}.{unitName}";
        string requestDeclaration = hasCustomRegistrar
            ? $"public sealed record {featureName} : global::Mu.Modelling.Behavior.Creational<global::{unitNamespace}.{unitName}>, global::Mu.Serialization.IBinder, global::Mu.Composition.IRegistrar"
            : $"public sealed partial record {featureName}";
        string registrar = hasCustomRegistrar
            ? $$"""
                {
                    public static int Registrations { get; private set; }

                    public static bool WasLocked { get; private set; }

                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        var metadata = model.Add(typeof({{featureName}}), false);
                        metadata.UseConstructor = false;
                        metadata.Add(1, nameof(Identity));
                        metadata.Add(2, nameof(Proposed));
                        metadata.Add(3, nameof(Model));

                        return model;
                    }

                    static void global::Mu.Composition.IRegistrar.Register(
                        global::Microsoft.Extensions.Configuration.IConfiguration configuration,
                        global::SimpleInjector.Container container)
                    {
                        Registrations++;
                        WasLocked = container.IsLocked;
                        _ = Bind(global::ProtoBuf.Meta.RuntimeTypeModel.Default);
                        container.RegisterInstance(new RegistrationMarker(configuration));
                        var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<{{featureName}}Options>(
                            configuration.GetSection(nameof({{featureName}}Options))) ?? new {{featureName}}Options();

                        if (options.Mode == global::Mu.Configuration.ModeType.Hosted && options.Hosted.Ipc == global::Mu.Configuration.IpcType.Grpc)
                        {
                            _ = ContainerExtensions.RegisterIpcHost(container, options.Hosted);
                        }
                    }
                }

                public sealed class RegistrationMarker(global::Microsoft.Extensions.Configuration.IConfiguration configuration)
                {
                    public global::Microsoft.Extensions.Configuration.IConfiguration Configuration { get; } = configuration;
                }
                """
            : ";";

        return $$"""
            namespace Muify.Domain
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class UnitAttribute<TIdentity> : global::System.Attribute
                    where TIdentity : struct;
            }

            namespace Muify.Service
            {
                [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
                public sealed class CreationalAttribute<TFact> : global::System.Attribute;
            }

            namespace {{unitNamespace}}
            {
                [global::Muify.Domain.Unit<global::System.Guid>]
                public sealed record {{unitName}} : global::Mu.Modelling.State.Aggregate;
            }

            namespace {{unitNamespace}}.{{featureName}}
            {
                using global::Mu.Composition;

                [global::Muify.Service.Creational<{{featureName}}ed>]
                {{requestDeclaration}}{{registrar}}

                public static class CompositionExample
                {
                    public static global::Microsoft.AspNetCore.Builder.WebApplication Build(global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
                    {
                        return builder.BuildMu().Build{{featureName}}();
                    }
                }
            }
            """;
    }

    private static string GetBuildFeatureNamespace(string unitName, string featureName)
    {
        return $"{BuildAreaNamespace}.{unitName}.{featureName}";
    }

    private static GrpcMethodMetadata[] GetBuildGrpcMethods(WebApplication application)
    {
        return ((IEndpointRouteBuilder)application).DataSources
            .SelectMany(source => source.Endpoints)
            .Select(endpoint => endpoint.Metadata.GetMetadata<GrpcMethodMetadata>())
            .OfType<GrpcMethodMetadata>()
            .ToArray();
    }

    private static WebApplication InvokeFeatureBuild(Assembly assembly, string featureNamespace, string featureName, WebApplication application)
    {
        Type extensions = assembly.GetType($"{featureNamespace}.{BuildExtensionType}", throwOnError: true)!;
        MethodInfo method = extensions.GetMethod($"Build{featureName}", [typeof(WebApplication)])!;

        return (WebApplication)method.Invoke(default, [application])!;
    }

    private static void RegisterBuildWriteStore(Container container, Type aggregate)
    {
        Type contract = typeof(IWriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        container.RegisterInstance(contract, Substitute.For([contract], []));
    }
}