namespace Muify.ModelGeneratorTests;

using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mu;
using Mu.Auditing;
using Mu.Communications.Mediation;
using Mu.Composition;
using Mu.Modelling.State;
using Mu.Persistence;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Meta;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using DependencyScope = SimpleInjector.Scope;

public sealed partial class WhenInitializeIsCalled
{
    private const string HostingAreaNamespace = "MooVC.Testing.Executables";

    private const string HostingPayload = "A generated feature can create and reload its aggregate.";

    [Test]
    public async Task GivenAnExecutableFeatureWithAReferencedDomainWhenOnlyItsBuilderExtensionIsCalledThenGrpcCreatesAndPersistsItsAggregate()
    {
        // Arrange
        const string featureName = "Open";
        const string unitName = "AccountExecutableFeature";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, featureName),
            featureNamespace,
            OutputKind.ConsoleApplication,
            "Definitions.cs",
            domain.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(featureName);

        // Act
        await using WebApplication application = InvokeHostingBuild(feature, featureNamespace, featureName, builder);
        Container container = application.GetMuContainer()!;
        bool isLockedBeforeStartup = container.IsLocked;
        await application.StartAsync();
        string address = GetHostingAddress(application);
        Result<Guid> result = await SendHostingCreation(feature, featureNamespace, featureName, address);
        Type aggregate = domain.Assembly.GetType($"{unitNamespace}.{unitName}", throwOnError: true)!;
        object? persisted = await GetHostingAggregate(container, aggregate, result.Value);
        bool isLockedAfterStartup = container.IsLocked;
        await application.StopAsync();

        // Assert
        _ = await Assert.That(feature.Assembly.EntryPoint).IsNotNull();
        _ = await Assert.That(feature.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{featureNamespace}.Program.g.cs");
        _ = await Assert.That(builder.GetMuContainer()).IsSameReferenceAs(container);
        _ = await Assert.That(isLockedBeforeStartup).IsFalse();
        _ = await Assert.That(isLockedAfterStartup).IsTrue();
        _ = await Assert.That(result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Value).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That(persisted).IsNotNull();
        _ = await Assert.That(aggregate.GetProperty("Name")!.GetValue(persisted)).IsEqualTo(HostingPayload);
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(aggregate)).IsTrue();
        _ = await Assert.That(container.GetInstance<IAuditor>()).IsTypeOf<InMemoryAuditor>();

        using (DependencyScope scope = AsyncScopedLifestyle.BeginScope(container))
        {
            _ = await Assert.That(container.GetInstance<IMediator>()).IsTypeOf<InMemoryMediator>();
        }
    }

    [Test]
    [Arguments(HostingAreaNamespace, "AccountExecutableHost")]
    [Arguments("MooVC.Testing.Platform.Payments.Executables", "AccountExecutableScopedHost")]
    public async Task GivenAHostWithTwoReferencedFeaturesForOneDomainThenItsBuilderComposesBothFeaturesAndRegistersTheDomainOnce(string areaNamespace, string unitName)
    {
        // Arrange
        const string firstFeature = "Open";
        const string secondFeature = "Start";
        const int expectedBindings = 1;
        const int expectedServices = 2;
        string unrelatedName = $"{areaNamespace}.{unitName}.Unrelated";
        string unitNamespace = $"{areaNamespace}.{unitName}";
        string firstNamespace = $"{unitNamespace}.{firstFeature}";
        string secondNamespace = $"{unitNamespace}.{secondFeature}";
        string hostNamespace = $"{unitNamespace}.Host";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName, countBindings: true, areaNamespace: areaNamespace), unitNamespace);
        HostingProject first = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, firstFeature),
            firstNamespace,
            OutputKind.ConsoleApplication,
            "Definitions.cs",
            domain.Reference);
        HostingProject second = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, secondFeature),
            secondNamespace,
            OutputKind.ConsoleApplication,
            "Definitions.cs",
            domain.Reference);
        HostingProject unrelated = await CompileHostingProject($"namespace {unrelatedName}; public sealed class Dependency;", unrelatedName);
        HostingProject host = await CompileHostingProject(
            CreateHostingHostSource(hostNamespace),
            hostNamespace,
            OutputKind.ConsoleApplication,
            "Definitions.cs",
            domain.Reference,
            second.Reference,
            unrelated.Reference,
            first.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(firstFeature, secondFeature);

        // Act
        await using WebApplication application = InvokeHostingBuild(host, hostNamespace, unitName, builder);
        Container container = application.GetMuContainer()!;
        bool isLockedBeforeStartup = container.IsLocked;
        await application.StartAsync();
        string address = GetHostingAddress(application);
        Result<Guid> firstResult = await SendHostingCreation(first, firstNamespace, firstFeature, address);
        Result<Guid> secondResult = await SendHostingCreation(second, secondNamespace, secondFeature, address);
        Type aggregate = domain.Assembly.GetType($"{unitNamespace}.{unitName}", throwOnError: true)!;
        object? firstAggregate = await GetHostingAggregate(container, aggregate, firstResult.Value);
        object? secondAggregate = await GetHostingAggregate(container, aggregate, secondResult.Value);
        int bindings = (int)aggregate.GetProperty("Bindings")!.GetValue(default)!;
        int services = GetBuildGrpcMethods(application).Select(method => method.ServiceType).Distinct().Count();
        await application.StopAsync();

        // Assert
        _ = await Assert.That(host.Assembly.EntryPoint).IsNotNull();
        _ = await Assert.That(host.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{hostNamespace}.Program.g.cs");
        _ = await Assert.That(isLockedBeforeStartup).IsFalse();
        _ = await Assert.That(bindings).IsEqualTo(expectedBindings);
        _ = await Assert.That(services).IsEqualTo(expectedServices);
        _ = await Assert.That(firstResult.IsSuccessful).IsTrue();
        _ = await Assert.That(secondResult.IsSuccessful).IsTrue();
        _ = await Assert.That(firstResult.Value).IsNotEqualTo(secondResult.Value);
        _ = await Assert.That(firstAggregate).IsNotNull();
        _ = await Assert.That(secondAggregate).IsNotNull();
        _ = await Assert.That(aggregate.GetProperty("Name")!.GetValue(firstAggregate)).IsEqualTo(HostingPayload);
        _ = await Assert.That(aggregate.GetProperty("Name")!.GetValue(secondAggregate)).IsEqualTo(HostingPayload);
        GeneratedSourceResult extension = await Assert.That(host.Result.GeneratedSources.Where(source => source.HintName == $"{hostNamespace}.WebApplicationExtensions.Build{unitName}.g.cs")).HasSingleItem();
        string expected = $$"""
            #nullable enable

            namespace {{hostNamespace}};

            public static partial class WebApplicationExtensions
            {
                extension(global::Microsoft.AspNetCore.Builder.WebApplication app)
                {
                    public global::Microsoft.AspNetCore.Builder.WebApplication Build{{unitName}}()
                    {
                        _ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, app, nameof(app), message: "The application must be provided.");

                        var container = global::Mu.Composition.WebApplicationExtensions.GetMuContainer(app) ?? throw new global::System.InvalidOperationException("Mu must be added before building the host.");

                        _ = global::{{firstNamespace}}.WebApplicationExtensions.Build{{firstFeature}}(app);
                        _ = global::{{secondNamespace}}.WebApplicationExtensions.Build{{secondFeature}}(app);
                        _ = global::Mu.Composition.ContainerExtensions.RegisterDefaults(container, app.Configuration);

                        return app;
                    }
                }
            }
            """;
        _ = await Assert.That(extension.SourceText.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("My.Project.Host", "Project", "AccountShortHost")]
    [Arguments("My.Product.Domain.Account.Host", "Account", "AccountLongHost")]
    public async Task GivenAHostNamespaceWithAnyNumberOfSegmentsThenItsExtensionsUseTheNameBeforeHost(string hostNamespace, string extensionName, string unitName)
    {
        // Arrange
        const string featureName = "Open";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(CreateHostingFeatureSource(unitNamespace, featureName), featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);

        // Act
        HostingProject host = await CompileHostingProject(
            CreateHostingHostSource(hostNamespace),
            hostNamespace,
            OutputKind.ConsoleApplication,
            "Definitions.cs",
            domain.Reference,
            feature.Reference);

        // Assert
        _ = await Assert.That(host.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{hostNamespace}.WebApplicationBuilderExtensions.Build{extensionName}.g.cs");
        _ = await Assert.That(host.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{hostNamespace}.WebApplicationExtensions.Build{extensionName}.g.cs");
        _ = await Assert.That(host.Assembly.EntryPoint).IsNotNull();
        GeneratedSourceResult program = await Assert.That(host.Result.GeneratedSources.Where(source => source.HintName == $"{hostNamespace}.Program.g.cs")).HasSingleItem();
        string expected = $$"""
            #nullable enable

            namespace {{hostNamespace}};

            internal static class Program
            {
                public static async global::System.Threading.Tasks.Task Main(string[] args)
                {
                    var builder = global::Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
                    await using var app = global::{{hostNamespace}}.WebApplicationBuilderExtensions.Build{{extensionName}}(builder);
                    await app.RunAsync().ConfigureAwait(false);
                }
            }
            """;
        _ = await Assert.That(program.SourceText.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("file", "AccountProgramFile")]
    [Arguments("top-level", "AccountTopLevelProgram")]
    [Arguments("main", "AccountExplicitMain")]
    public async Task GivenAnExistingExecutableEntryPointThenTheFeatureProgramIsNotGenerated(string kind, string unitName)
    {
        // Arrange
        const string featureName = "Open";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        string source = CreateHostingFeatureSource(unitNamespace, featureName);
        string entryPoint = kind == "top-level"
            ? "global::System.Console.WriteLine(global::System.String.Empty);"
            : "namespace Custom { internal static class EntryPoint { public static void Main() { } } }";
        source = kind == "top-level" ? entryPoint + Environment.NewLine + source : source + Environment.NewLine + entryPoint;
        string path = kind == "file" ? "Program.cs" : "Definitions.cs";

        // Act
        HostingProject feature = await CompileHostingProject(source, featureNamespace, OutputKind.ConsoleApplication, path, domain.Reference);

        // Assert
        _ = await Assert.That(feature.Assembly.EntryPoint).IsNotNull();
        _ = await Assert.That(feature.Result.GeneratedSources.Any(definition => definition.HintName == $"{featureNamespace}.Program.g.cs")).IsFalse();
        _ = await Assert.That(feature.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{featureNamespace}.WebApplicationBuilderExtensions.Build{featureName}.g.cs");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenALibraryFeatureOrHostThenBuildExtensionsAreGeneratedWithoutAnEntryPoint(bool isHost)
    {
        // Arrange
        const string featureName = "Open";
        string unitName = isHost ? "AccountLibraryHost" : "AccountLibraryFeature";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(CreateHostingFeatureSource(unitNamespace, featureName), featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);

        // Act
        HostingProject project = isHost
            ? await CompileHostingProject(CreateHostingHostSource($"{unitNamespace}.Host"), $"{unitNamespace}.Host", OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference, feature.Reference)
            : feature;

        // Assert
        _ = await Assert.That(project.Assembly.EntryPoint).IsNull();
        _ = await Assert.That(project.Result.GeneratedSources.Any(definition => definition.HintName.EndsWith(".Program.g.cs", StringComparison.Ordinal))).IsFalse();
        _ = await Assert.That(project.Result.GeneratedSources.Any(definition => definition.HintName.Contains(".WebApplicationBuilderExtensions.Build", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task GivenAHostWithAnExistingMainThenItsProgramIsNotGenerated()
    {
        // Arrange
        const string featureName = "Open";
        const string unitName = "AccountHostMain";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        string hostNamespace = $"{unitNamespace}.Host";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(CreateHostingFeatureSource(unitNamespace, featureName), featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);
        string source = CreateHostingHostSource(hostNamespace) + " internal static class CustomProgram { public static void Main() { } }";

        // Act
        HostingProject host = await CompileHostingProject(source, hostNamespace, OutputKind.ConsoleApplication, "Definitions.cs", domain.Reference, feature.Reference);

        // Assert
        _ = await Assert.That(host.Assembly.EntryPoint!.DeclaringType!.Name).IsEqualTo("CustomProgram");
        _ = await Assert.That(host.Result.GeneratedSources.Any(definition => definition.HintName.EndsWith(".Program.g.cs", StringComparison.Ordinal))).IsFalse();
        _ = await Assert.That(host.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{hostNamespace}.WebApplicationBuilderExtensions.Build{unitName}.g.cs");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenACustomFeatureBuildExtensionThenItsReceiverOverridesOnlyTheMatchingGeneratedExtension(bool isBuilder)
    {
        // Arrange
        const string featureName = "Open";
        const int expectedCalls = 1;
        string unitName = isBuilder ? "AccountCustomBuilder" : "AccountCustomApplication";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        string extensions = isBuilder ? "WebApplicationBuilderExtensions" : "WebApplicationExtensions";
        string receiver = isBuilder ? "WebApplicationBuilder" : "WebApplication";
        string result = isBuilder ? "global::Mu.Composition.WebApplicationBuilderExtensions.BuildMu(app)" : "app";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        string source = CreateHostingFeatureSource(unitNamespace, featureName) + $$"""

            namespace {{featureNamespace}}
            {
                public static partial class {{extensions}}
                {
                    public static int Calls { get; private set; }

                    public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(
                        this global::Microsoft.AspNetCore.Builder.{{receiver}} app)
                    {
                        Calls++;
                        return {{result}};
                    }
                }
            }
            """;
        HostingProject feature = await CompileHostingProject(source, featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(featureName);

        // Act
        await using WebApplication application = InvokeHostingBuild(feature, featureNamespace, featureName, builder);
        Type customExtensions = feature.Assembly.GetType($"{featureNamespace}.{extensions}", throwOnError: true)!;
        int calls = (int)customExtensions.GetProperty("Calls")!.GetValue(default)!;

        // Assert
        _ = await Assert.That(calls).IsEqualTo(expectedCalls);
        _ = await Assert.That(feature.Result.GeneratedSources.Any(definition => definition.HintName == $"{featureNamespace}.WebApplicationBuilderExtensions.Build{featureName}.g.cs")).IsEqualTo(!isBuilder);
        _ = await Assert.That(feature.Result.GeneratedSources.Any(definition => definition.HintName == $"{featureNamespace}.WebApplicationExtensions.Build{featureName}.g.cs")).IsEqualTo(isBuilder);
        _ = await Assert.That(application.GetMuContainer()).IsNotNull();
    }

    [Test]
    public async Task GivenAnApplicationBuildOverrideThatThrowsThenTheBuilderDisposesItsApplicationAndPreservesTheFailure()
    {
        // Arrange
        const string featureName = "Open";
        const string expectedMessage = "The custom application composition failed.";
        const string unitName = "AccountFailedBuilder";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        string source = CreateHostingFeatureSource(unitNamespace, featureName) + $$"""

            namespace {{featureNamespace}}
            {
                public static partial class WebApplicationExtensions
                {
                    public static global::Microsoft.AspNetCore.Builder.WebApplication Application { get; private set; } = default!;

                    public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(
                        this global::Microsoft.AspNetCore.Builder.WebApplication app)
                    {
                        Application = app;
                        throw new global::System.InvalidOperationException("{{expectedMessage}}");
                    }
                }
            }
            """;
        HostingProject feature = await CompileHostingProject(source, featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(featureName);

        // Act
        Exception? failure = CaptureBuildFailure(() => _ = InvokeHostingBuild(feature, featureNamespace, featureName, builder));
        Type extensions = feature.Assembly.GetType($"{featureNamespace}.WebApplicationExtensions", throwOnError: true)!;
        await using var application = (WebApplication)extensions.GetProperty("Application")!.GetValue(default)!;
        ObjectDisposedException? disposed = default;

        try
        {
            _ = application.Services.GetService(typeof(IServiceProvider));
        }
        catch (ObjectDisposedException exception)
        {
            disposed = exception;
        }

        // Assert
        _ = await Assert.That(failure).IsTypeOf<InvalidOperationException>();
        _ = await Assert.That(failure!.Message).IsEqualTo(expectedMessage);
        _ = await Assert.That(disposed).IsNotNull();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenACustomHostBuildExtensionThenItsReceiverOverridesOnlyTheMatchingGeneratedExtension(bool isBuilder)
    {
        // Arrange
        const string featureName = "Open";
        const int expectedCalls = 1;
        string unitName = isBuilder ? "AccountCustomHostBuilder" : "AccountCustomHostApplication";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        string hostNamespace = $"{unitNamespace}.Host";
        string extensions = isBuilder ? "WebApplicationBuilderExtensions" : "WebApplicationExtensions";
        string receiver = isBuilder ? "WebApplicationBuilder" : "WebApplication";
        string result = isBuilder ? "global::Mu.Composition.WebApplicationBuilderExtensions.BuildMu(app)" : "app";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(CreateHostingFeatureSource(unitNamespace, featureName), featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);
        string source = $$"""
            namespace {{hostNamespace}}
            {
                public static partial class {{extensions}}
                {
                    public static int Calls { get; private set; }

                    public static global::Microsoft.AspNetCore.Builder.WebApplication Build{{unitName}}(
                        this global::Microsoft.AspNetCore.Builder.{{receiver}} app)
                    {
                        Calls++;
                        return {{result}};
                    }
                }
            }
            """;
        HostingProject host = await CompileHostingProject(source, hostNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference, feature.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(featureName);

        // Act
        await using WebApplication application = InvokeHostingBuild(host, hostNamespace, unitName, builder);
        Type customExtensions = host.Assembly.GetType($"{hostNamespace}.{extensions}", throwOnError: true)!;
        int calls = (int)customExtensions.GetProperty("Calls")!.GetValue(default)!;

        // Assert
        _ = await Assert.That(calls).IsEqualTo(expectedCalls);
        _ = await Assert.That(host.Result.GeneratedSources.Any(definition => definition.HintName == $"{hostNamespace}.WebApplicationBuilderExtensions.Build{unitName}.g.cs")).IsEqualTo(!isBuilder);
        _ = await Assert.That(host.Result.GeneratedSources.Any(definition => definition.HintName == $"{hostNamespace}.WebApplicationExtensions.Build{unitName}.g.cs")).IsEqualTo(isBuilder);
        _ = await Assert.That(application.GetMuContainer()).IsNotNull();
    }

    [Test]
    public async Task GivenAnExistingBuildExtensionWithAnUnrelatedReceiverThenTheFeatureApplicationExtensionIsStillGenerated()
    {
        // Arrange
        const string featureName = "Open";
        const string unitName = "AccountUnrelatedReceiver";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        string source = CreateHostingFeatureSource(unitNamespace, featureName) + $$"""

            namespace {{featureNamespace}}
            {
                public static partial class WebApplicationExtensions
                {
                    public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(this string app)
                        => throw new global::System.NotSupportedException();
                }
            }
            """;

        // Act
        HostingProject feature = await CompileHostingProject(source, featureNamespace, OutputKind.DynamicallyLinkedLibrary, "Definitions.cs", domain.Reference);

        // Assert
        _ = await Assert.That(feature.Result.GeneratedSources.Select(source => source.HintName)).Contains($"{featureNamespace}.WebApplicationExtensions.Build{featureName}.g.cs");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenGeneratedFeatureOrHostCompositionWhenGenerationIsRepeatedThenTheEntrypointAndExtensionsAreNotDuplicated(bool isHost)
    {
        // Arrange
        const string featureName = "Open";
        string unitName = isHost ? "AccountRepeatedHost" : "AccountRepeatedFeature";
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(CreateHostingFeatureSource(unitNamespace, featureName), featureNamespace, OutputKind.ConsoleApplication, "Definitions.cs", domain.Reference);
        HostingProject project = isHost
            ? await CompileHostingProject(CreateHostingHostSource($"{unitNamespace}.Host"), $"{unitNamespace}.Host", OutputKind.ConsoleApplication, "Definitions.cs", domain.Reference, feature.Reference)
            : feature;

        // Act
        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(project.Compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources).IsEmpty();
    }

    private static async Task<HostingProject> CompileHostingProject(
        string source,
        string assemblyName,
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
        string path = "Definitions.cs",
        params MetadataReference[] references)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source, path: path)],
            _references.Concat(references),
            new CSharpCompilationOptions(outputKind));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        using var stream = new MemoryStream();
        var emitted = generated.Emit(stream);
        _ = await Assert.That(emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        byte[] image = stream.ToArray();
        stream.Position = 0;
        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(stream);

        return new HostingProject(assembly, generated, result, MetadataReference.CreateFromImage(image));
    }

    private static WebApplicationBuilder CreateHostingBuilder(params string[] featureNames)
    {
        const string dynamicPort = "0";
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        var values = featureNames.ToDictionary(
            name => $"{name}Options:Hosted:Grpc:Port",
            _ => (string?)dynamicPort);
        _ = builder.Configuration.AddInMemoryCollection(values);

        return builder;
    }

    private static string CreateHostingDomainSource(string unitName, bool countBindings = false, string areaNamespace = HostingAreaNamespace)
    {
        string binderContract = countBindings ? ", global::Mu.Serialization.IBinder" : string.Empty;
        string binder = countBindings
            ? $$"""
                public static int Bindings { get; private set; }

                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    Bindings++;
                    var metadata = model.Add(typeof({{unitName}}), false);
                    metadata.UseConstructor = false;
                    metadata.Add(1, nameof(Name));
                    return model;
                }
                """
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

            namespace {{areaNamespace}}.{{unitName}}
            {
                [global::Muify.Domain.Unit<global::System.Guid>]
                public sealed partial record {{unitName}} : global::Mu.Modelling.State.Aggregate{{binderContract}}
                {
                    public string Name { get; init; } = global::System.String.Empty;

                    {{binder}}
                }
            }
            """;
    }

    private static string CreateHostingFeatureSource(string unitNamespace, string featureName)
    {
        return $$"""
            namespace {{unitNamespace}}.{{featureName}}
            {
                [global::Muify.Service.Creational<{{featureName}}ed>]
                public sealed partial record {{featureName}}
                {
                    public string Name { get; init; } = global::System.String.Empty;
                }
            }
            """;
    }

    private static string CreateHostingHostSource(string hostNamespace)
    {
        return $$"""
            namespace {{hostNamespace}};

            public sealed class HostMarker;
            """;
    }

    private static async Task<object?> GetHostingAggregate(Container container, Type aggregate, Guid identity)
    {
        Type contract = typeof(IWriteStore<,>).MakeGenericType(aggregate, typeof(Guid));
        using DependencyScope scope = AsyncScopedLifestyle.BeginScope(container);
        object store = container.GetInstance(contract);
        var pending = (Task)contract.GetMethod(nameof(IWriteStore<Aggregate, Guid>.Get))!.Invoke(store, [identity, ulong.MaxValue, CancellationToken.None])!;
        await pending;

        return pending.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(pending);
    }

    private static string GetHostingAddress(WebApplication application)
    {
        IServer server = application.Services.GetRequiredService<IServer>();
        string address = server.Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        var endpoint = new UriBuilder(address) { Host = "localhost" };

        return endpoint.Uri.ToString();
    }

    private static WebApplication InvokeHostingBuild(HostingProject project, string projectNamespace, string name, WebApplicationBuilder builder)
    {
        Type extensions = project.Assembly.GetType($"{projectNamespace}.WebApplicationBuilderExtensions", throwOnError: true)!;
        MethodInfo method = extensions.GetMethod($"Build{name}", [typeof(WebApplicationBuilder)])!;

        return (WebApplication)method.Invoke(default, [builder])!;
    }

    private static async Task<Result<Guid>> SendHostingCreation(HostingProject feature, string featureNamespace, string featureName, string address)
    {
        Type requestType = feature.Assembly.GetType($"{featureNamespace}.{featureName}", throwOnError: true)!;
        Type service = feature.Assembly.GetType($"{featureNamespace}.I{featureName}Service+IGrpc", throwOnError: true)!;
        object request = Activator.CreateInstance(requestType)!;
        requestType.GetProperty("Name")!.SetValue(request, HostingPayload);
        using GrpcChannel channel = GrpcChannel.ForAddress(address);
        MethodInfo creation = typeof(GrpcClientFactory).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(GrpcClientFactory.CreateGrpcService)
                && method.IsGenericMethodDefinition
                && method.GetParameters()[0].ParameterType == typeof(ChannelBase));
        object client = creation.MakeGenericMethod(service).Invoke(default, [channel, default])!;
        var pending = (ValueTask<Result<Guid>>)service.GetMethod(featureName)!.Invoke(client, [request, default(CallContext)])!;

        return await pending;
    }

    private sealed record HostingProject(Assembly Assembly, Compilation Compilation, GeneratorRunResult Result, MetadataReference Reference);
}