namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseModelIsCalled
{
    private const string HostAssemblyName = "MooVC.Testing.Mechanics.Account.Host";

    [Test]
    [Arguments("Product.Host", "BuildProduct")]
    [Arguments("MooVC.Testing.Host", "BuildTesting")]
    [Arguments(HostAssemblyName, "BuildAccount")]
    public async Task GivenATerminalHostNamespaceThenItsApplicationIsDiscoveredAtAnySupportedDepth(string assemblyName, string buildName)
    {
        // Arrange
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.IsHost).IsTrue();
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(assemblyName);
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(buildName);
        _ = await Assert.That(result.Areas).IsEmpty();
        _ = await Assert.That(result.Metadata.Features).IsEmpty();
    }

    [Test]
    public async Task GivenAHostRootNamespaceWhenTheAssemblyNameDiffersThenTheConfiguredNamespaceIsUsed()
    {
        // Arrange
        const string assemblyName = "Custom.Runtime";
        const string rootNamespace = "Custom.Component.Host";
        const string buildName = "BuildComponent";
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName);

        // Act
        Model result = compilation.ParseModel(rootNamespace, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.IsHost).IsTrue();
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(rootNamespace);
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(buildName);
    }

    [Test]
    [Arguments("MooVC.Testing.Host.Account")]
    [Arguments("MooVC.Testing.Mechanics.host")]
    public async Task GivenANonTerminalOrDifferentlyCasedHostNameThenItIsNotAHost(string assemblyName)
    {
        // Arrange
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.IsHost).IsFalse();
    }

    [Test]
    public async Task GivenAFeatureWhenItsRootNamespaceDiffersThenItsCanonicalFeatureNamespaceAndBuildNameAreUsed()
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Mechanics.Car.Open";
        const string rootNamespace = "Custom.Component";
        const string buildName = "BuildOpen";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                public sealed partial record Open;
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source);

        // Act
        Model result = compilation.ParseModel(rootNamespace, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.IsHost).IsFalse();
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(assemblyName);
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(buildName);
        Qualification feature = await Assert.That(result.Metadata.Features).HasSingleItem();
        _ = await Assert.That(feature).IsEqualTo((Qualification)(Name: "Open", Qualifier: assemblyName));
    }

    [Test]
    [Arguments(OutputKind.ConsoleApplication, true)]
    [Arguments(OutputKind.WindowsApplication, true)]
    [Arguments(OutputKind.DynamicallyLinkedLibrary, false)]
    public async Task GivenAnOutputKindThenExecutableMetadataMatchesTheProjectKind(OutputKind outputKind, bool expected)
    {
        // Arrange
        CSharpCompilation compilation = CreateApplicationCompilation(HostAssemblyName, outputKind: outputKind);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.IsExecutable).IsEqualTo(expected);
    }

    [Test]
    [Arguments("Program.cs", false, false, true)]
    [Arguments("Nested/program.cs", false, false, true)]
    [Arguments("Other.cs", true, false, true)]
    [Arguments("Other.cs", false, true, true)]
    [Arguments("Other.cs", false, false, false)]
    public async Task GivenAnEntryFileTopLevelStatementsOrMainThenExistingProgramsArePreserved(string filePath, bool hasStatements, bool hasMain, bool expected)
    {
        // Arrange
        const string statements = "_ = global::System.Environment.TickCount;";
        const string entryPoint = "public static class EntryPoint { public static void Main() { } }";
        string source = hasMain ? entryPoint : string.Empty;

        if (hasStatements)
        {
            source = statements;
        }

        CSharpCompilation compilation = CreateApplicationCompilation(HostAssemblyName, source, OutputKind.ConsoleApplication, filePath: filePath);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.HasProgram).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenReferencedProjectsThenOnlyAccessibleGenuineFeaturesWithCallableApplicationBuildMethodsAreCatalogued()
    {
        // Arrange
        const string firstAssemblyName = "Other.Company.Area.First.Open";
        const string secondAssemblyName = "MooVC.Testing.Mechanics.Second.Read";
        const string unrelatedAssemblyName = "MooVC.Testing.Mechanics.Other.Ignore";
        const string inaccessibleAssemblyName = "MooVC.Testing.Mechanics.Hidden.Hide";
        const string invalidAssemblyName = "MooVC.Testing.Mechanics.Invalid.Skip";
        const string classicMethod = "public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(this global::Microsoft.AspNetCore.Builder.WebApplication app) { return app; }";
        const string modernMethod = "extension(global::Microsoft.AspNetCore.Builder.WebApplication app) { public global::Microsoft.AspNetCore.Builder.WebApplication BuildRead() { return app; } }";
        const string unrelatedMethod = "public static global::Microsoft.AspNetCore.Builder.WebApplication BuildIgnore(this global::Microsoft.AspNetCore.Builder.WebApplication app) { return app; }";
        const string inaccessibleMethod = "public static global::Microsoft.AspNetCore.Builder.WebApplication BuildHide(this global::Microsoft.AspNetCore.Builder.WebApplication app) { return app; }";
        const string invalidMethod = "public static global::Microsoft.AspNetCore.Builder.WebApplication BuildSkip(this global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder) { return default!; }";
        MetadataReference first = await CreateApplicationReference(firstAssemblyName, CreateHostedReferenceSource(firstAssemblyName, classicMethod));
        MetadataReference second = await CreateApplicationReference(secondAssemblyName, CreateHostedReferenceSource(secondAssemblyName, modernMethod));
        MetadataReference unrelated = await CreateApplicationReference(unrelatedAssemblyName, CreateHostedReferenceSource(unrelatedAssemblyName, unrelatedMethod, hasBase: false));
        MetadataReference inaccessible = await CreateApplicationReference(inaccessibleAssemblyName, CreateHostedReferenceSource(inaccessibleAssemblyName, inaccessibleMethod, isAccessible: false));
        MetadataReference invalid = await CreateApplicationReference(invalidAssemblyName, CreateHostedReferenceSource(invalidAssemblyName, invalidMethod));
        CSharpCompilation compilation = CreateApplicationCompilation(HostAssemblyName).AddReferences(second, unrelated, first, inaccessible, invalid);
        Qualification[] expected =
        [
            (Name: "Read", Qualifier: secondAssemblyName),
            (Name: "Open", Qualifier: firstAssemblyName),
        ];

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.Features.SequenceEqual(expected)).IsTrue();
    }

    [Test]
    public async Task GivenExistingHostBuildExtensionsThenBothApplicationAndBuilderOverridesAreDiscovered()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Account.Host;

            public static class WebApplicationExtensions
            {
                public static global::Microsoft.AspNetCore.Builder.WebApplication BuildAccount(
                    this global::Microsoft.AspNetCore.Builder.WebApplication app)
                {
                    return app;
                }
            }

            public static class WebApplicationBuilderExtensions
            {
                extension(global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
                {
                    public global::Microsoft.AspNetCore.Builder.WebApplication BuildAccount()
                    {
                        return builder.Build();
                    }
                }
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(HostAssemblyName, source);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Metadata.HasApplicationBuildExtension).IsTrue();
        _ = await Assert.That(result.Metadata.HasBuilderBuildExtension).IsTrue();
    }

    [Test]
    [Arguments(false, false, false, false)]
    [Arguments(true, true, false, false)]
    [Arguments(true, false, false, true)]
    [Arguments(false, false, true, true)]
    [Arguments(true, true, true, true)]
    public async Task GivenFeatureGenerationEligibilityOrACustomBuilderThenOnlyAvailableBuildMethodsReceiveAProgram(
        bool isPartial,
        bool hasIncompatibleOptions,
        bool hasCustomBuilder,
        bool expected)
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Mechanics.Car.Open";
        const string buildName = "BuildOpen";
        const string incompatibleOptions = "public sealed record OpenOptions;";
        const string customBuilder = """
            public static class WebApplicationBuilderExtensions
            {
                public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(
                    this global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)
                {
                    return builder.Build();
                }
            }
            """;
        string modifier = isPartial ? "partial" : string.Empty;
        string options = hasIncompatibleOptions ? incompatibleOptions : string.Empty;
        string builder = hasCustomBuilder ? customBuilder : string.Empty;
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                public sealed {{modifier}} record Open : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>;

                {{options}}

                {{builder}}
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source, OutputKind.ConsoleApplication);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);
        GeneratorRunResult generated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(expected ? buildName : string.Empty);
        _ = await Assert.That(result.Metadata.HasBuilderBuildExtension).IsEqualTo(hasCustomBuilder);
        _ = await Assert.That(generated.Diagnostics).IsEmpty();
        _ = await Assert.That(generated.GeneratedSources.Any(definition => definition.HintName == $"{assemblyName}.Program.g.cs")).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenInaccessibleOptionsThenTheUnavailableBuildMethodDoesNotReceiveAProgram()
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Mechanics.Car.Open";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                public sealed partial record Open : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>;

                public static class Configuration
                {
                    private sealed record CustomOptions : global::Mu.Configuration.Options;
                }
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source, OutputKind.ConsoleApplication);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);
        GeneratorRunResult generated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(string.Empty);
        _ = await Assert.That(generated.Diagnostics).IsEmpty();
        _ = await Assert.That(generated.GeneratedSources.Any(definition => definition.HintName == $"{assemblyName}.Program.g.cs")).IsFalse();
    }

    [Test]
    public async Task GivenANonPartialFeatureWithoutARegistrarWhenItsApplicationBuildIsOverriddenThenItsBuilderAndProgramUseTheOverride()
    {
        // Arrange
        const string assemblyName = "MooVC.Testing.Mechanics.Car.Open";
        const string buildName = "BuildOpen";
        const string expectedCall = "global::MooVC.Testing.Mechanics.Car.Open.WebApplicationExtensions.BuildOpen(app)";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Open
            {
                public sealed record Open : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public sealed record Result;
                }

                public static class WebApplicationExtensions
                {
                    public static global::Microsoft.AspNetCore.Builder.WebApplication BuildOpen(
                        this global::Microsoft.AspNetCore.Builder.WebApplication app)
                    {
                        return app;
                    }
                }
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source, OutputKind.ConsoleApplication);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult definitions = driver.GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(buildName);
        _ = await Assert.That(result.Metadata.HasApplicationBuildExtension).IsTrue();
        _ = await Assert.That(definitions.Diagnostics).IsEmpty();
        _ = await Assert.That(definitions.GeneratedSources.Any(definition => definition.HintName == $"{assemblyName}.Program.g.cs")).IsTrue();
        _ = await Assert.That(definitions.GeneratedSources.Any(definition => definition.HintName == $"{assemblyName}.WebApplicationExtensions.{buildName}.g.cs")).IsFalse();
        GeneratedSourceResult builder = await Assert.That(definitions.GeneratedSources.Where(definition => definition.HintName == $"{assemblyName}.WebApplicationBuilderExtensions.{buildName}.g.cs")).HasSingleItem();
        _ = await Assert.That(builder.SourceText.ToString()).Contains(expectedCall);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    private static CSharpCompilation CreateApplicationCompilation(
        string assemblyName,
        string? source = default,
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
        string filePath = "Application.cs")
    {
        return CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source ?? string.Empty, path: filePath)],
            _references,
            new CSharpCompilationOptions(outputKind));
    }

    private static async Task<MetadataReference> CreateApplicationReference(string assemblyName, string source)
    {
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source);
        using var stream = new MemoryStream();
        var emitted = compilation.Emit(stream);
        _ = await Assert.That(emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        stream.Position = 0;

        return MetadataReference.CreateFromStream(stream);
    }

    private static string CreateHostedReferenceSource(string assemblyName, string methods, bool hasBase = true, bool isAccessible = true)
    {
        string[] segments = assemblyName.Split('.');
        string feature = segments[^1];
        string unit = segments[^2];
        string unitNamespace = string.Join('.', segments[..^1]);
        string accessibility = isAccessible ? "public" : "internal";
        string inheritance = hasBase ? $": global::Mu.Modelling.Behavior.Query<global::{unitNamespace}.{unit}>" : string.Empty;

        return $$"""
            namespace {{unitNamespace}}
            {
                public sealed record {{unit}} : global::Mu.Modelling.State.Aggregate;
            }

            namespace {{assemblyName}}
            {
                {{accessibility}} sealed record {{feature}} {{inheritance}};

                public static class WebApplicationExtensions
                {
                    {{methods}}
                }
            }
            """;
    }
}