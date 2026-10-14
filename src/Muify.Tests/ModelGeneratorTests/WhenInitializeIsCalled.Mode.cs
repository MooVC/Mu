namespace Muify.ModelGeneratorTests;

using System.Globalization;
using System.Reflection;
using global::Grpc.AspNetCore.Server;
using global::Grpc.Net.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mu;
using Mu.Auditing;
using Mu.Communications.Mediation;
using Mu.Communications.Mediation.Composition;
using Mu.Communications.Tracing;
using Mu.Configuration;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Persistence;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;
using Options = Mu.Configuration.Options;

public sealed partial class WhenInitializeIsCalled
{
    private const string ModeAreaNamespace = "MooVC.Testing.Composition";

    private const string ModeRegistrationMethod = "RegisterMode";

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenTwoHostedFeaturesWhenTheGlobalMediatorIsRegisteredBeforeOrAfterThemThenBothGrpcServicesCanBeResolved(bool registerGlobalFirst)
    {
        // Arrange
        string firstUnit = registerGlobalFirst ? "VehicleFirstEarlyMediator" : "VehicleFirstLateMediator";
        string secondUnit = registerGlobalFirst ? "VehicleSecondEarlyMediator" : "VehicleSecondLateMediator";
        string firstNamespace = GetModeFeatureNamespace(firstUnit);
        string secondNamespace = GetModeFeatureNamespace(secondUnit);
        Assembly firstAssembly = await CompileRegistrar(CreateModeFeatureSource(firstUnit), firstNamespace);
        Assembly secondAssembly = await CompileRegistrar(CreateModeFeatureSource(secondUnit), secondNamespace);
        Type firstAggregate = firstAssembly.GetType($"{ModeAreaNamespace}.{firstUnit}.{firstUnit}", throwOnError: true)!;
        Type secondAggregate = secondAssembly.GetType($"{ModeAreaNamespace}.{secondUnit}.{secondUnit}", throwOnError: true)!;
        Type firstService = firstAssembly.GetType($"{firstNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        Type secondService = secondAssembly.GetType($"{secondNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        using Container container = CreateModeContainer(firstAggregate);
        Type secondStore = typeof(IWriteStore<,>).MakeGenericType(secondAggregate, typeof(Guid));
        container.RegisterInstance(secondStore, Substitute.For([secondStore], []));

        if (registerGlobalFirst)
        {
            _ = container.RegisterMediator();
        }

        // Act
        _ = InvokeModeRegistration(firstAssembly, firstNamespace, container, new Options(), typeof(Options));
        _ = InvokeModeRegistration(secondAssembly, secondNamespace, container, new Options(), typeof(Options));

        if (!registerGlobalFirst)
        {
            _ = container.RegisterMediator();
        }

        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        object first = container.GetInstance(firstService);
        object second = container.GetInstance(secondService);
        IMediator mediator = container.GetInstance<IMediator>();

        // Assert
        _ = await Assert.That(first.GetType()).IsEqualTo(firstService);
        _ = await Assert.That(second.GetType()).IsEqualTo(secondService);
        _ = await Assert.That(mediator).IsTypeOf<InMemoryMediator>();
    }

    [Test]
    [Arguments(false, false)]
    [Arguments(false, true)]
    [Arguments(true, false)]
    [Arguments(true, true)]
    public async Task GivenPositionalOrRequiredOptionsWhenTheirSectionIsPresentOrAbsentThenConfigurationOrCoreDefaultsSelectTheMode(bool isPositional, bool hasSection)
    {
        // Arrange
        const string description = "Configured feature";
        const string optionsName = "CustomOptions";
        const string original = "internal sealed record CustomOptions : global::Mu.Configuration.Options;";
        const string positional = "internal sealed record CustomOptions(string Description) : global::Mu.Configuration.Options;";
        const string required = "internal sealed record CustomOptions : global::Mu.Configuration.Options { public required string Description { get; init; } }";
        string unitName = $"VehicleOptions{(isPositional ? "Positional" : "Required")}{(hasSection ? "Configured" : "Default")}";
        string featureNamespace = GetModeFeatureNamespace(unitName);
        string source = CreateModeFeatureSource(unitName, hasCustomOptions: true).Replace(original, isPositional ? positional : required, StringComparison.Ordinal);
        Assembly assembly = await CompileRegistrar(source, featureNamespace);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{unitName}.{unitName}", throwOnError: true)!;
        Type grpcService = assembly.GetType($"{featureNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        Type serviceContract = assembly.GetType($"{featureNamespace}.IOpenService", throwOnError: true)!;
        var values = new Dictionary<string, string?>();

        if (hasSection)
        {
            values[$"{optionsName}:Description"] = description;
            values[$"{optionsName}:{nameof(Options.Mode)}"] = nameof(ModeType.Remote);
        }

        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        using Container container = CreateModeContainer(aggregate);

        // Act
        object result = InvokeModeRegistration(assembly, featureNamespace, container, configuration, typeof(IConfiguration));
        bool hasHostedService = container.GetCurrentRegistrations().Any(registration => registration.ServiceType == grpcService);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(hasHostedService).IsEqualTo(!hasSection);

        if (hasSection)
        {
            using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
            object client = container.GetInstance(serviceContract);
            _ = await Assert.That(client.GetType()).IsEqualTo(assembly.GetType($"{featureNamespace}.OpenService+Grpc+Client"));
        }
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenHostedConfigurationWhenAMediatorOverrideIsPresentOrAbsentThenItsServicesAndSettingsAreRegistered(bool hasCustomMediator)
    {
        // Arrange
        const string customUnit = "VehicleCustomMediator";
        const string defaultUnit = "VehicleHosted";
        const string optionsName = "OpenOptions";
        const string transformName = "Transform";
        const int receiveLimit = 1024;
        const int sendLimit = 2048;
        string unitName = hasCustomMediator ? customUnit : defaultUnit;
        string featureNamespace = GetModeFeatureNamespace(unitName);
        Assembly assembly = await CompileRegistrar(CreateModeFeatureSource(unitName), featureNamespace);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{unitName}.{unitName}", throwOnError: true)!;
        Type request = assembly.GetType($"{featureNamespace}.Open", throwOnError: true)!;
        Type fact = assembly.GetType($"{featureNamespace}.Opened", throwOnError: true)!;
        Type grpcService = assembly.GetType($"{featureNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        Type unrelatedConsumer = assembly.GetType($"{featureNamespace}.UnrelatedMediatorConsumer", throwOnError: true)!;
        Type serviceContract = typeof(IService<,>).MakeGenericType(request, typeof(Guid));
        Type handlerContract = typeof(IHandler<,>).MakeGenericType(request, typeof(Guid));
        Type rootContract = typeof(IRoot<,>).MakeGenericType(aggregate, request);
        Type invariantContract = typeof(IInvariant<,>).MakeGenericType(aggregate, request);
        Type transformContract = typeof(ITransform<,>).MakeGenericType(aggregate, fact);
        var values = new Dictionary<string, string?>
        {
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.EnableDetailedErrors)}"] = bool.TrueString,
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxReceiveMessageSize)}"] = receiveLimit.ToString(CultureInfo.InvariantCulture),
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.MaxSendMessageSize)}"] = sendLimit.ToString(CultureInfo.InvariantCulture),
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        using Container container = CreateModeContainer(aggregate);
        IMediator customMediator = Substitute.For<IMediator>();

        if (hasCustomMediator)
        {
            Registration registration = Lifestyle.Singleton.CreateRegistration(() => customMediator, container);
            container.RegisterConditional<IMediator>(
                registration,
                context => context.HasConsumer && context.Consumer.ImplementationType == grpcService);
        }

        // Act
        object result = InvokeModeRegistration(assembly, featureNamespace, container, configuration, typeof(IConfiguration));
        object first;
        object repeated;
        object mediator;
        object root;
        object handler;
        object[] invariants;
        object[] transforms;
        Result<Guid> outcome;
        Exception? directFailure;
        Exception? unrelatedFailure;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            first = container.GetInstance(grpcService);
            repeated = container.GetInstance(grpcService);
            InstanceProducer mediatorProducer = container.GetRegistration(grpcService, throwOnFailure: true)!
                .GetRelationships()
                .Single(relationship => relationship.Dependency.ServiceType == typeof(IMediator))
                .Dependency;
            mediator = mediatorProducer.GetInstance();
            root = container.GetInstance(rootContract);
            handler = container.GetInstance(handlerContract);
            invariants = container.GetAllInstances(invariantContract).ToArray();
            transforms = container.GetAllInstances(transformContract).ToArray();
            object service = container.GetInstance(serviceContract);
            MethodInfo execute = serviceContract.GetMethod(nameof(IService<object, Guid>.Execute))!;
            var execution = (Task<Result<Guid>>)execute.Invoke(service, [Activator.CreateInstance(request), CancellationToken.None])!;
            outcome = await execution;
            directFailure = CaptureModeFailure(() => _ = container.GetInstance<IMediator>());
            unrelatedFailure = CaptureModeFailure(() => _ = container.GetInstance(unrelatedConsumer));
        }

        object next;
        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            next = container.GetInstance(grpcService);
        }

        Type settingsType = typeof(GrpcServiceOptions<>).MakeGenericType(grpcService);
        Type settingsContract = typeof(IOptions<>).MakeGenericType(settingsType);
        object configured = container.GetInstance(settingsContract);
        object settings = settingsContract.GetProperty(nameof(IOptions<object>.Value))!.GetValue(configured)!;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(repeated).IsSameReferenceAs(first);
        _ = await Assert.That(ReferenceEquals(next, first)).IsFalse();
        _ = await Assert.That(root.GetType()).IsEqualTo(typeof(Root<,,>).MakeGenericType(aggregate, fact, request));
        _ = await Assert.That(handler.GetType()).IsEqualTo(typeof(ServiceHandler<,>).MakeGenericType(request, typeof(Guid)));
        _ = await Assert.That(invariants).IsEmpty();
        object transform = await Assert.That(transforms).HasSingleItem();
        _ = await Assert.That(transform.GetType().Name).IsEqualTo(transformName);
        _ = await Assert.That(outcome.IsSuccessful).IsTrue();
        _ = await Assert.That(outcome.Value).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(directFailure).IsTypeOf<ActivationException>();
        _ = await Assert.That(unrelatedFailure).IsTypeOf<ActivationException>();
        _ = await Assert.That((bool?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.EnableDetailedErrors))!.GetValue(settings)).IsTrue();
        _ = await Assert.That((int?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.MaxReceiveMessageSize))!.GetValue(settings)).IsEqualTo(receiveLimit);
        _ = await Assert.That((int?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.MaxSendMessageSize))!.GetValue(settings)).IsEqualTo(sendLimit);

        if (hasCustomMediator)
        {
            _ = await Assert.That(mediator).IsSameReferenceAs(customMediator);
        }
        else
        {
            _ = await Assert.That(mediator).IsTypeOf<InMemoryMediator>();
        }
    }

    [Test]
    public async Task GivenRemoteConfigurationThenOnlyItsScopedClientAndSingletonProxyAreRegistered()
    {
        // Arrange
        const string address = "http://localhost:50053";
        const string optionsName = "OpenOptions";
        const string unitName = "VehicleRemote";
        string featureNamespace = GetModeFeatureNamespace(unitName);
        Assembly assembly = await CompileRegistrar(CreateModeFeatureSource(unitName), featureNamespace);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{unitName}.{unitName}", throwOnError: true)!;
        Type request = assembly.GetType($"{featureNamespace}.Open", throwOnError: true)!;
        Type publicContract = assembly.GetType($"{featureNamespace}.IOpenService", throwOnError: true)!;
        Type grpcContract = publicContract.GetNestedType("IGrpc", BindingFlags.Public)!;
        Type expectedClient = assembly.GetType($"{featureNamespace}.OpenService+Grpc+Client", throwOnError: true)!;
        Type grpcService = assembly.GetType($"{featureNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        var values = new Dictionary<string, string?>
        {
            [$"{optionsName}:{nameof(Options.Mode)}"] = nameof(ModeType.Remote),
            [$"{optionsName}:{nameof(Options.Remote)}:{nameof(Remote.Grpc)}:{nameof(Remote.GrpcOptions.Address)}"] = address,
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        using Container container = CreateModeContainer(aggregate);

        // Act
        object result = InvokeModeRegistration(assembly, featureNamespace, container, configuration, typeof(IConfiguration));
        object first;
        object repeated;
        object proxy;

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            first = container.GetInstance(publicContract);
            repeated = container.GetInstance(publicContract);
            proxy = container.GetInstance(grpcContract);
        }

        object next;
        object nextProxy;
        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            next = container.GetInstance(publicContract);
            nextProxy = container.GetInstance(grpcContract);
        }

        GrpcChannel channel = container.ContainerScope.GetDisposables().OfType<GrpcChannel>().Single();
        Type serviceContract = typeof(IService<,>).MakeGenericType(request, typeof(Guid));
        Type handlerContract = typeof(IHandler<,>).MakeGenericType(request, typeof(Guid));
        Type rootContract = typeof(IRoot<,>).MakeGenericType(aggregate, request);

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That(first.GetType()).IsEqualTo(expectedClient);
        _ = await Assert.That(repeated).IsSameReferenceAs(first);
        _ = await Assert.That(ReferenceEquals(next, first)).IsFalse();
        _ = await Assert.That(nextProxy).IsSameReferenceAs(proxy);
        _ = await Assert.That(channel.Target).IsEqualTo(new Uri(address).Authority);
        _ = await Assert.That(container.GetRegistration(grpcContract, throwOnFailure: true)!.Lifestyle).IsSameReferenceAs(Lifestyle.Singleton);
        _ = await Assert.That(container.GetRegistration(serviceContract, throwOnFailure: false)).IsNull();
        _ = await Assert.That(container.GetRegistration(handlerContract, throwOnFailure: false)).IsNull();
        _ = await Assert.That(container.GetRegistration(rootContract, throwOnFailure: false)).IsNull();
        _ = await Assert.That(container.GetCurrentRegistrations().Any(registration => registration.ServiceType == grpcService)).IsFalse();
    }

    [Test]
    [Arguments(ModeType.Hosted, IpcType.None)]
    [Arguments(ModeType.Remote, IpcType.None)]
    [Arguments((ModeType)int.MaxValue, IpcType.Grpc)]
    public async Task GivenUnsupportedModeOrIpcThenRegistrationThrowsNotSupportedException(ModeType mode, IpcType ipc)
    {
        // Arrange
        const string unitPrefix = "VehicleUnsupported";
        const string ipcMessage = "The configured IPC transport is not supported.";
        const string modeMessage = "The configured feature mode is not supported.";
        string unitName = $"{unitPrefix}{mode}";
        string featureNamespace = GetModeFeatureNamespace(unitName);
        Assembly assembly = await CompileRegistrar(CreateModeFeatureSource(unitName), featureNamespace);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{unitName}.{unitName}", throwOnError: true)!;
        var options = new Options
        {
            Mode = mode,
            Hosted = new Hosted { Ipc = ipc },
            Remote = new Remote { Ipc = ipc },
        };
        using Container container = CreateModeContainer(aggregate);

        // Act
        Exception? failure = CaptureModeFailure(() => _ = InvokeModeRegistration(assembly, featureNamespace, container, options, typeof(Options)));

        // Assert
        _ = await Assert.That(failure).IsTypeOf<NotSupportedException>();
        _ = await Assert.That(failure!.Message).IsEqualTo(mode is ModeType.Hosted or ModeType.Remote ? ipcMessage : modeMessage);
    }

    [Test]
    public async Task GivenInternalCustomOptionsThenTheirNamedSectionIsBoundWithoutExposingTheType()
    {
        // Arrange
        const string optionsName = "CustomOptions";
        const string unitName = "VehicleInternalOptions";
        string featureNamespace = GetModeFeatureNamespace(unitName);
        Assembly assembly = await CompileRegistrar(CreateModeFeatureSource(unitName, hasCustomOptions: true), featureNamespace);
        Type aggregate = assembly.GetType($"{ModeAreaNamespace}.{unitName}.{unitName}", throwOnError: true)!;
        Type grpcService = assembly.GetType($"{featureNamespace}.OpenService+Grpc+Service", throwOnError: true)!;
        Type settingsType = typeof(GrpcServiceOptions<>).MakeGenericType(grpcService);
        Type settingsContract = typeof(IOptions<>).MakeGenericType(settingsType);
        var values = new Dictionary<string, string?>
        {
            [$"{optionsName}:{nameof(Options.Hosted)}:{nameof(Hosted.Grpc)}:{nameof(Hosted.GrpcOptions.EnableDetailedErrors)}"] = bool.TrueString,
        };
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        using Container container = CreateModeContainer(aggregate);

        // Act
        object result = InvokeModeRegistration(assembly, featureNamespace, container, configuration, typeof(IConfiguration));
        object configured = container.GetInstance(settingsContract);
        object settings = settingsContract.GetProperty(nameof(IOptions<object>.Value))!.GetValue(configured)!;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(container);
        _ = await Assert.That((bool?)settingsType.GetProperty(nameof(GrpcServiceOptions<object>.EnableDetailedErrors))!.GetValue(settings)).IsTrue();
    }

    [Test]
    public async Task GivenGeneratedCSharpExtensionsWhenGenerationIsRepeatedThenModeAndIpcRegistrationsAreNotDuplicated()
    {
        // Arrange
        const string unitName = "VehicleRepeatedGeneration";
        const string extensionHint = ".ContainerExtensions.Register";
        const int expectedExtensionCount = 3;
        string featureNamespace = GetModeFeatureNamespace(unitName);
        var compilation = CSharpCompilation.Create(
            featureNamespace,
            [CSharpSyntaxTree.ParseText(CreateModeFeatureSource(unitName))],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult initial = driver.GetRunResult().Results.Single();

        // Act
        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(initial.Diagnostics).IsEmpty();
        _ = await Assert.That(initial.GeneratedSources.Count(source => source.HintName.Contains(extensionHint, StringComparison.Ordinal))).IsEqualTo(expectedExtensionCount);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources.Any(source => source.HintName.Contains(extensionHint, StringComparison.Ordinal))).IsFalse();
    }

    private static Exception? CaptureModeFailure(Action action)
    {
        try
        {
            action();
        }
        catch (TargetInvocationException exception)
        {
            return exception.InnerException;
        }
        catch (ActivationException exception)
        {
            return exception;
        }

        return default;
    }

    private static Container CreateModeContainer(Type aggregate)
    {
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;
        container.RegisterInstance(Substitute.For<IScopeManager>());
        container.RegisterInstance(Substitute.For<IScribe>());
        container.RegisterInstance(Substitute.For<ILogger<InMemoryMediator>>());
        container.RegisterInstance<IServiceProvider>(container);
        Type store = typeof(IWriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        container.RegisterInstance(store, Substitute.For([store], []));

        return container;
    }

    private static string CreateModeFeatureSource(string unitName, bool hasCustomOptions = false)
    {
        string unitNamespace = $"{ModeAreaNamespace}.{unitName}";
        string customOptions = hasCustomOptions
            ? "internal sealed record CustomOptions : global::Mu.Configuration.Options;"
            : string.Empty;

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

            namespace {{unitNamespace}}.Open
            {
                [global::Muify.Service.Creational<Opened>]
                public sealed partial record Open;

                {{customOptions}}

                public sealed class UnrelatedMediatorConsumer(global::Mu.Communications.Mediation.IMediator mediator)
                {
                    public global::Mu.Communications.Mediation.IMediator Mediator { get; } = mediator;
                }
            }
            """;
    }

    private static string GetModeFeatureNamespace(string unitName)
    {
        return $"{ModeAreaNamespace}.{unitName}.Open";
    }

    private static object InvokeModeRegistration(Assembly assembly, string featureNamespace, Container container, object options, Type optionsType)
    {
        Type extensions = assembly.GetType($"{featureNamespace}.ContainerExtensions", throwOnError: true)!;
        MethodInfo registration = extensions.GetMethod(ModeRegistrationMethod, [typeof(Container), optionsType])!;

        return registration.Invoke(default, [container, options])!;
    }
}