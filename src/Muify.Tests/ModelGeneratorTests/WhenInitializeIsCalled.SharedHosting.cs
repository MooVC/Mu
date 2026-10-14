namespace Muify.ModelGeneratorTests;

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.CodeAnalysis;
using Mu.Composition;
using Mu.Serialization;
using ProtoBuf.Meta;

public sealed partial class WhenInitializeIsCalled
{
    private const int SharedCustomFieldNumber = 17;

    [Test]
    public async Task GivenAStandaloneFeatureWithASharedLibraryThenItsBuilderRegistersUnusedAndInternalContracts()
    {
        // Arrange
        const string featureName = "Open";
        const string libraryNamespace = "MooVC.Testing.Shared.StandaloneContracts";
        const string unitName = "AccountSharedStandalone";
        const int expectedBindings = 1;
        const int expectedGeneratedFieldNumber = 4;
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        HostingProject library = await CompileHostingProject(CreateSharedHostingLibrarySource(libraryNamespace), libraryNamespace);
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject feature = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, featureName),
            featureNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(featureName);
        Type details = library.Assembly.GetType($"{libraryNamespace}.Details", throwOnError: true)!;
        Type internalDetails = library.Assembly.GetType($"{libraryNamespace}.InternalDetails", throwOnError: true)!;
        Type statistics = library.Assembly.GetType($"{libraryNamespace}.Statistics", throwOnError: true)!;
        Type customDetails = library.Assembly.GetType($"{libraryNamespace}.CustomDetails", throwOnError: true)!;

        // Act
        await using WebApplication application = InvokeHostingBuild(feature, featureNamespace, featureName, builder);

        // Assert
        _ = await Assert.That(application.GetMuContainer()).IsNotNull();

        foreach (Type contract in new[] { details, internalDetails, statistics })
        {
            _ = await Assert.That(typeof(IBinder).IsAssignableFrom(contract)).IsTrue();
            _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(contract)).IsTrue();
            ValueMember field = await Assert.That(RuntimeTypeModel.Default[contract].GetFields()).HasSingleItem();
            _ = await Assert.That(field.FieldNumber).IsEqualTo(expectedGeneratedFieldNumber);
        }

        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(customDetails)).IsTrue();
        ValueMember customField = await Assert.That(RuntimeTypeModel.Default[customDetails].GetFields()).HasSingleItem();
        _ = await Assert.That(customField.FieldNumber).IsEqualTo(SharedCustomFieldNumber);
        _ = await Assert.That(customDetails.GetProperty("Bindings")!.GetValue(default)).IsEqualTo(expectedBindings);
        _ = await Assert.That(feature.Result.GeneratedSources.Any(source => source.HintName.StartsWith(libraryNamespace, StringComparison.Ordinal))).IsFalse();
    }

    [Test]
    public async Task GivenAHostWithTwoFeaturesReferencingOneSharedLibraryThenItsBuilderRegistersTheLibraryBeforeTheFeatures()
    {
        // Arrange
        const string firstFeature = "Open";
        const string secondFeature = "Start";
        const string libraryNamespace = "MooVC.Testing.Shared.HostContracts";
        const string unitName = "AccountSharedHost";
        const int expectedBindings = 1;
        const int expectedServices = 2;
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string firstNamespace = $"{unitNamespace}.{firstFeature}";
        string secondNamespace = $"{unitNamespace}.{secondFeature}";
        string hostNamespace = $"{unitNamespace}.Host";
        HostingProject library = await CompileHostingProject(CreateSharedHostingLibrarySource(libraryNamespace), libraryNamespace);
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject first = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, firstFeature),
            firstNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference);
        HostingProject second = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, secondFeature),
            secondNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference);
        HostingProject host = await CompileHostingProject(
            CreateHostingHostSource(hostNamespace),
            hostNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            second.Reference,
            library.Reference,
            first.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(firstFeature, secondFeature);
        Type customDetails = library.Assembly.GetType($"{libraryNamespace}.CustomDetails", throwOnError: true)!;

        // Act
        await using WebApplication application = InvokeHostingBuild(host, hostNamespace, unitName, builder);

        // Assert
        _ = await Assert.That(customDetails.GetProperty("Bindings")!.GetValue(default)).IsEqualTo(expectedBindings);
        _ = await Assert.That(GetBuildGrpcMethods(application).Select(method => method.ServiceType).Distinct().Count()).IsEqualTo(expectedServices);
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

                        _ = global::Mu.Serialization.RuntimeTypeModelExtensions.BindOnce<global::{{libraryNamespace}}.Binder>(global::ProtoBuf.Meta.RuntimeTypeModel.Default);
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
    public async Task GivenASharedLibraryWithACustomBinderWhenTwoFeaturesAreComposedThenTheCustomBinderRunsOnce()
    {
        // Arrange
        const string firstFeature = "Open";
        const string secondFeature = "Start";
        const string libraryNamespace = "MooVC.Testing.Shared.CustomContracts";
        const string binderName = "Binder";
        const string unitName = "AccountSharedCustomHost";
        const int expectedRegistrations = 1;
        string unitNamespace = $"{HostingAreaNamespace}.{unitName}";
        string firstNamespace = $"{unitNamespace}.{firstFeature}";
        string secondNamespace = $"{unitNamespace}.{secondFeature}";
        string hostNamespace = $"{unitNamespace}.Host";
        string source = CreateSharedHostingLibrarySource(libraryNamespace) + $$"""

            namespace {{libraryNamespace}}
            {
                public sealed class {{binderName}} : global::Mu.Serialization.IBinder
                {
                    public static int Registrations { get; private set; }

                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        Registrations++;
                        _ = Details.Bind(model);
                        _ = InternalDetails.Bind(model);
                        _ = Statistics.Bind(model);
                        _ = CustomDetails.Bind(model);

                        return model;
                    }
                }
            }
            """;
        HostingProject library = await CompileHostingProject(source, libraryNamespace);
        HostingProject domain = await CompileHostingProject(CreateHostingDomainSource(unitName), unitNamespace);
        HostingProject first = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, firstFeature),
            firstNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference);
        HostingProject second = await CompileHostingProject(
            CreateHostingFeatureSource(unitNamespace, secondFeature),
            secondNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference);
        HostingProject host = await CompileHostingProject(
            CreateHostingHostSource(hostNamespace),
            hostNamespace,
            OutputKind.DynamicallyLinkedLibrary,
            "Definitions.cs",
            domain.Reference,
            library.Reference,
            first.Reference,
            second.Reference);
        WebApplicationBuilder builder = CreateHostingBuilder(firstFeature, secondFeature);
        Type binder = library.Assembly.GetType($"{libraryNamespace}.{binderName}", throwOnError: true)!;
        Type customDetails = library.Assembly.GetType($"{libraryNamespace}.CustomDetails", throwOnError: true)!;
        MethodInfo bindOnce = typeof(RuntimeTypeModelExtensions).GetMethod(nameof(RuntimeTypeModelExtensions.BindOnce))!.MakeGenericMethod(binder);

        // Act
        await using WebApplication application = InvokeHostingBuild(host, hostNamespace, unitName, builder);
        _ = bindOnce.Invoke(default, [RuntimeTypeModel.Default]);

        // Assert
        _ = await Assert.That(binder.GetProperty("Registrations")!.GetValue(default)).IsEqualTo(expectedRegistrations);
        _ = await Assert.That(customDetails.GetProperty("Bindings")!.GetValue(default)).IsEqualTo(expectedRegistrations);
        _ = await Assert.That(library.Result.GeneratedSources.Any(definition => definition.HintName == $"{libraryNamespace}.Binder.g.cs")).IsFalse();
        _ = await Assert.That(RuntimeTypeModel.Default.IsDefined(customDetails)).IsTrue();
    }

    private static string CreateSharedHostingLibrarySource(string libraryNamespace)
    {
        return $$"""
            namespace {{libraryNamespace}}
            {
                public sealed partial record Details(string Value);

                internal sealed partial record InternalDetails(string Value);

                public readonly partial record struct Statistics(int Value);

                public sealed record CustomDetails(string Value) : global::Mu.Serialization.IBinder
                {
                    public static int Bindings { get; private set; }

                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        Bindings++;
                        var metadata = model.Add(typeof(CustomDetails), false);
                        metadata.UseConstructor = false;
                        metadata.Add({{SharedCustomFieldNumber}}, nameof(Value));

                        return model;
                    }
                }
            }
            """;
    }
}