namespace Muify.ModelGeneratorTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    public async Task GivenAPartialEntityThenAllGeneratedMembersCompileTogether()
    {
        // Arrange
        const string source = """
            namespace Muify.Domain
            {
                public sealed class IdentityAttribute : System.Attribute;
            }

            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car(Wheel Wheel, Details Details);

                public sealed partial record Details(string Value);

                public sealed partial class Wheel
                {
                    [Muify.Domain.Identity]
                    public int Location { get; set; }
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        string[] hints = result.GeneratedSources.Select(definition => definition.HintName).ToArray();
        _ = await Assert.That(hints).Contains("Wheel.Binder.g.cs");
        _ = await Assert.That(hints).Contains("Details.Binder.g.cs");
        _ = await Assert.That(hints).Contains("Wheel.Equals.g.cs");
        _ = await Assert.That(hints).Contains("Wheel.GetHashCode.g.cs");
        _ = await Assert.That(hints).Contains("Wheel.Identifier.IEquatable.Equals.g.cs");
        _ = await Assert.That(hints).Contains("Wheel.Self.IEquatable.Equals.g.cs");
        _ = await Assert.That(hints).Contains("Wheel.Self.IComparable.CompareTo.g.cs");
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureThenAllServiceVisitorsRunAndGeneratedSourcesCompileTogether()
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Search";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace MooVC.Testing.Mechanics.Car.Search
            {
                public sealed partial record Search
                {
                    public sealed record Result(string Value);
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            featureAssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation generated, out _);
        GeneratorRunResult result = driver.GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        string[] hints = result.GeneratedSources.Select(definition => definition.HintName).ToArray();
        _ = await Assert.That(hints).Contains("SearchService.g.cs");
        _ = await Assert.That(hints).Contains("ISearchService.g.cs");
        _ = await Assert.That(hints).Contains("SearchService.Grpc.Client.g.cs");
        _ = await Assert.That(hints).Contains("SearchService.Grpc.Service.g.cs");
        _ = await Assert.That(hints).Contains("ISearchService.Grpc.g.cs");
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        GeneratorRunResult repeated = CSharpGeneratorDriver.Create(new ModelGenerator()).RunGenerators(generated).GetRunResult().Results.Single();
        _ = await Assert.That(repeated.Diagnostics).IsEmpty();
        _ = await Assert.That(repeated.GeneratedSources).IsEmpty();
    }
}