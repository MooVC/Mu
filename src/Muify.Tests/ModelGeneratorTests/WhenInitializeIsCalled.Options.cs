namespace Muify.ModelGeneratorTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed partial class WhenInitializeIsCalled
{
    private const string OptionsAssemblyName = "MooVC.Testing.Mechanics.Car.Register";

    private const string OptionsHint = "MooVC.Testing.Mechanics.Car.Register.RegisterOptions.g.cs";

    [Test]
    [Arguments(false, false)]
    [Arguments(false, true)]
    [Arguments(true, false)]
    [Arguments(true, true)]
    public async Task GivenAFeatureWithoutOptionsThenItsDefaultConfigurationCompiles(bool hasGenericOptions, bool isPartialFeature)
    {
        // Arrange
        const string expectedBase = "Mu.Configuration.Options";
        const string expectedName = "MooVC.Testing.Mechanics.Car.Register.RegisterOptions";
        const string expectedPartialDeclaration = "public sealed partial record RegisterOptions";
        string declaration = hasGenericOptions ? "public sealed partial record RegisterOptions<TValue>;" : string.Empty;
        CSharpCompilation compilation = CreateOptionsCompilation(declaration, isPartialFeature);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult options = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).HasSingleItem();
        _ = await Assert.That(options.SourceText.ToString()).Contains(expectedPartialDeclaration);
        Compilation generated = compilation.AddSyntaxTrees(options.SyntaxTree);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        INamedTypeSymbol optionsType = generated.GetTypeByMetadataName(expectedName)!;
        _ = await Assert.That(optionsType.IsRecord).IsTrue();
        _ = await Assert.That(optionsType.IsSealed).IsTrue();
        _ = await Assert.That(optionsType.DeclaredAccessibility).IsEqualTo(Accessibility.Public);
        _ = await Assert.That(optionsType.BaseType!.ToDisplayString()).IsEqualTo(expectedBase);

        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).IsEmpty();
    }

    [Test]
    [Arguments("public sealed partial record RegisterOptions;")]
    [Arguments("internal partial record RegisterOptions;")]
    [Arguments("public abstract partial record RegisterOptions;")]
    [Arguments("public partial record RegisterOptions(string Value);")]
    [Arguments("public partial record RegisterOptions : IMarker; public interface IMarker;")]
    public async Task GivenPartialConventionalOptionsThenTheirGeneratedBaseCompiles(string declaration)
    {
        // Arrange
        const string expectedBase = "Mu.Configuration.Options";
        const string expectedName = "MooVC.Testing.Mechanics.Car.Register.RegisterOptions";
        const string expectedPartialDeclaration = "partial record RegisterOptions";
        CSharpCompilation compilation = CreateOptionsCompilation(declaration);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult options = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).HasSingleItem();
        _ = await Assert.That(options.SourceText.ToString()).Contains(expectedPartialDeclaration);
        Compilation generated = compilation.AddSyntaxTrees(options.SyntaxTree);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        INamedTypeSymbol optionsType = generated.GetTypeByMetadataName(expectedName)!;
        _ = await Assert.That(optionsType.BaseType!.ToDisplayString()).IsEqualTo(expectedBase);

        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).IsEmpty();
    }

    [Test]
    [Arguments("public sealed record RegisterOptions;")]
    [Arguments("public sealed partial class RegisterOptions;")]
    [Arguments("public partial interface RegisterOptions;")]
    [Arguments("public partial struct RegisterOptions;")]
    [Arguments("public partial record struct RegisterOptions;")]
    [Arguments("public sealed partial record RegisterOptions : OtherBase; public abstract record OtherBase;")]
    public async Task GivenConventionalOptionsThatCannotBeExtendedThenNoConfigurationIsGenerated(string declaration)
    {
        // Arrange
        CSharpCompilation compilation = CreateOptionsCompilation(declaration);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).IsEmpty();
    }

    [Test]
    [Arguments("public sealed record CustomOptions : Mu.Configuration.Options;")]
    [Arguments("public sealed record CustomOptions : OptionsBase;")]
    [Arguments("public static class Settings { public sealed record CustomOptions : OptionsBase; }")]
    [Arguments("public sealed record RegisterOptions : Mu.Configuration.Options;")]
    public async Task GivenExistingOptionsThenNoConfigurationIsGenerated(string declaration)
    {
        // Arrange
        CSharpCompilation compilation = CreateOptionsCompilation(declaration);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).IsEmpty();
    }

    [Test]
    public async Task GivenOptionsInADescendantNamespaceThenNoConfigurationIsGenerated()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register.Custom
            {
                public sealed record CustomOptions : OptionsBase;
            }
            """;
        CSharpCompilation compilation = CreateOptionsCompilation(string.Empty)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == OptionsHint)).IsEmpty();
    }

    private static CSharpCompilation CreateOptionsCompilation(string declaration, bool isPartialFeature = true)
    {
        string partial = isPartialFeature ? "partial" : string.Empty;
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;

                public abstract record OptionsBase : Mu.Configuration.Options;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed {{partial}} record Register : Mu.Modelling.Behavior.Query<Car>;

                {{declaration}}
            }
            """;

        return CSharpCompilation.Create(
            OptionsAssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}