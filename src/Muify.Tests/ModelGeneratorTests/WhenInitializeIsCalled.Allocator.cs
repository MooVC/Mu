namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Mu;
using Mu.Composition;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;
using Mu.Persistence;
using SimpleInjector;
using SimpleInjector.Lifestyles;

public sealed partial class WhenInitializeIsCalled
{
    private const string AllocatorFeatureAssemblyName = "MooVC.Testing.Mechanics.Car.Open";

    private const string AllocatorFeatureSource = """
        namespace Muify.Domain
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class UnitAttribute<TIdentity> : System.Attribute
                where TIdentity : struct;
        }

        namespace Muify.Service
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public sealed class CreationalAttribute<TFact> : System.Attribute;
        }

        namespace MooVC.Testing.Mechanics.Car
        {
            [Muify.Domain.Unit<System.Guid>]
            public sealed record Car : Mu.Modelling.State.Aggregate;
        }

        namespace MooVC.Testing.Mechanics.Car.Open
        {
            [Muify.Service.Creational<Opened>]
            public sealed partial record Open;

            public sealed class UnrelatedConsumer(Mu.Modelling.Services.IAllocator<System.Guid> allocator)
            {
                public Mu.Modelling.Services.IAllocator<System.Guid> Allocator { get; } = allocator;
            }
        }
        """;

    [Test]
    public async Task GivenAGuidCreationalFeatureWithoutAnAllocatorThenOnlyItsGeneratedServiceReceivesTheDefaultAllocator()
    {
        // Arrange
        const string featureName = "Allocate";
        const string featureAssemblyName = $"{AssemblyName}.{featureName}";
        const string unrelatedConsumerName = $"{featureAssemblyName}.UnrelatedConsumer";
        Assembly assembly = await CompileAllocatorFeature(featureName);
        Type request = assembly.GetType($"{featureAssemblyName}.{featureName}", throwOnError: true)!;
        Type unrelatedConsumer = assembly.GetType(unrelatedConsumerName, throwOnError: true)!;
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;

        // Act
        RegisterAllocatorFeature(container, request);
        Result<Guid> result;
        Exception? directFailure;
        Exception? unrelatedFailure;
        using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            result = await ExecuteAllocatedFeature(container, request);
            directFailure = CaptureAllocatorResolution(() => _ = container.GetInstance<IAllocator<Guid>>());
            unrelatedFailure = CaptureAllocatorResolution(() => _ = container.GetInstance(unrelatedConsumer));
        }

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Value).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(directFailure).IsTypeOf<ActivationException>();
        _ = await Assert.That(unrelatedFailure).IsTypeOf<ActivationException>();
    }

    [Test]
    public async Task GivenAnEarlierConditionalAllocatorThenTheGeneratedServiceUsesItsIdentity()
    {
        // Arrange
        const string featureName = "AllocateCustom";
        const string featureAssemblyName = $"{AssemblyName}.{featureName}";
        const string identityText = "f4d5c0d3-bd1d-43c4-ab5f-cb3194a281ed";
        Guid identity = Guid.Parse(identityText);
        Assembly assembly = await CompileAllocatorFeature(featureName);
        Type aggregate = assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type request = assembly.GetType($"{featureAssemblyName}.{featureName}", throwOnError: true)!;
        Type consumer = typeof(CreationalService<,,>).MakeGenericType(aggregate, typeof(Guid), request);
        IAllocator<Guid> allocator = new ConfiguredAllocator(identity);
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;
        Registration registration = Lifestyle.Singleton.CreateRegistration(() => allocator, container);
        container.RegisterConditional<IAllocator<Guid>>(
            registration,
            context => context.HasConsumer && context.Consumer.ImplementationType == consumer);

        // Act
        RegisterAllocatorFeature(container, request);
        using Scope scope = AsyncScopedLifestyle.BeginScope(container);
        Result<Guid> result = await ExecuteAllocatedFeature(container, request);

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Value).IsEqualTo(identity);
    }

    [Test]
    public async Task GivenTwoGuidCreationalFeatureRegistrarsThenEachGeneratedServiceReceivesItsDefaultAllocator()
    {
        // Arrange
        const string firstFeatureName = "AllocateTogether";
        const string firstFeatureAssemblyName = $"{AssemblyName}.{firstFeatureName}";
        const string secondFeatureName = "AllocateOther";
        const string secondFeatureAssemblyName = $"{AssemblyName}.{secondFeatureName}";
        Assembly firstAssembly = await CompileAllocatorFeature(firstFeatureName);
        Assembly secondAssembly = await CompileAllocatorFeature(secondFeatureName);
        Type firstRequest = firstAssembly.GetType($"{firstFeatureAssemblyName}.{firstFeatureName}", throwOnError: true)!;
        Type secondRequest = secondAssembly.GetType($"{secondFeatureAssemblyName}.{secondFeatureName}", throwOnError: true)!;
        using var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Options.EnableAutoVerification = false;

        // Act
        RegisterAllocatorFeature(container, firstRequest);
        RegisterAllocatorFeature(container, secondRequest);
        using Scope scope = AsyncScopedLifestyle.BeginScope(container);
        Result<Guid> first = await ExecuteAllocatedFeature(container, firstRequest);
        Result<Guid> second = await ExecuteAllocatedFeature(container, secondRequest);

        // Assert
        _ = await Assert.That(first.IsSuccessful).IsTrue();
        _ = await Assert.That(second.IsSuccessful).IsTrue();
        _ = await Assert.That(first.Value).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(second.Value).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(first.Value).IsNotEqualTo(second.Value);
    }

    [Test]
    public async Task GivenAReferencedUnitAndAFeatureAllocatorThenNoDefaultAllocatorIsGenerated()
    {
        // Arrange
        const string allocatorName = "GuidAllocator";
        const string registrarHint = $"{AllocatorFeatureAssemblyName}.Open.Registrar.g.cs";
        const string domainSource = """
            namespace Muify.Domain
            {
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class UnitAttribute<TIdentity> : System.Attribute
                    where TIdentity : struct;
            }

            namespace MooVC.Testing.Mechanics.Car
            {
                [Muify.Domain.Unit<System.Guid>]
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }
            """;
        const string featureSource = """
            namespace Muify.Service
            {
                [System.AttributeUsage(System.AttributeTargets.Class)]
                public sealed class CreationalAttribute<TFact> : System.Attribute;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                using System;
                using System.Threading;
                using System.Threading.Tasks;
                using Mu.Modelling.Behavior;
                using Mu.Modelling.Services;

                [Muify.Service.Creational<Opened>]
                public sealed partial record Open;

                public sealed class FeatureAllocator : IAllocator<Guid>
                {
                    public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
                        where TUseCase : UseCase
                        => ValueTask.FromResult(Guid.Empty);

                    public ValueTask Confirm(Guid identity, CancellationToken cancellationToken)
                        => ValueTask.CompletedTask;

                    public ValueTask Surrender(Guid identity, CancellationToken cancellationToken)
                        => ValueTask.CompletedTask;
                }
            }
            """;
        var domain = CSharpCompilation.Create(
            AssemblyName,
            [CSharpSyntaxTree.ParseText(domainSource)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var emitted = domain.Emit(stream);
        _ = await Assert.That(emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        stream.Position = 0;
        var compilation = CSharpCompilation.Create(
            AllocatorFeatureAssemblyName,
            [CSharpSyntaxTree.ParseText(featureSource)],
            _references.Append(MetadataReference.CreateFromStream(stream)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult registrar = await Assert.That(result.GeneratedSources.Where(source => source.HintName == registrarHint)).HasSingleItem();
        _ = await Assert.That(registrar.SourceText.ToString()).DoesNotContain(allocatorName);
    }

    private static Exception? CaptureAllocatorResolution(Action action)
    {
        try
        {
            action();
        }
        catch (ActivationException exception)
        {
            return exception;
        }

        return default;
    }

    private static Task<Assembly> CompileAllocatorFeature(string featureName)
    {
        const string originalFeatureName = "Open";

        return CompileRegistrar(
            AllocatorFeatureSource.Replace(originalFeatureName, featureName, StringComparison.Ordinal),
            $"{AssemblyName}.{featureName}");
    }

    private static async Task<Result<Guid>> ExecuteAllocatedFeature(Container container, Type request)
    {
        Type contract = typeof(IService<,>).MakeGenericType(request, typeof(Guid));
        object service = container.GetInstance(contract);
        MethodInfo execute = contract.GetMethod(nameof(IService<object, Guid>.Execute))!;
        var execution = (Task<Result<Guid>>)execute.Invoke(service, [Activator.CreateInstance(request), CancellationToken.None])!;

        return await execution;
    }

    private static void RegisterAllocatorFeature(Container container, Type request)
    {
        Type aggregate = request.Assembly.GetType($"{AssemblyName}.Car", throwOnError: true)!;
        Type store = typeof(IWriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        container.RegisterInstance(store, Substitute.For([store], []));
        IConfiguration configuration = new ConfigurationBuilder().Build();
        MethodInfo registrar = request.GetMethod(nameof(IRegistrar.Register))!;

        _ = registrar.Invoke(default, [configuration, container]);
    }

    private sealed class ConfiguredAllocator
        : IAllocator<Guid>
    {
        private readonly Guid _identity;

        public ConfiguredAllocator(Guid identity)
        {
            _identity = identity;
        }

        public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
            where TUseCase : UseCase
        {
            return ValueTask.FromResult(_identity);
        }

        public ValueTask Confirm(Guid identity, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Surrender(Guid identity, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
}