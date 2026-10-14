namespace Muify.ModelGeneratorTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed partial class WhenInitializeIsCalled
{
    private const string ViewSourceAssemblyName = "MooVC.Testing.Mechanics.Car.ReadSourceView";

    [Test]
    public async Task GivenANestedPartialResultThenItsBinderIsGeneratedInsideTheRequest()
    {
        // Arrange
        const string resultDeclaration = "public sealed partial record Result(string Value);";
        const string expectedHint = "MooVC.Testing.Mechanics.Car.ReadSourceView.ReadSourceView.Result.Binder.g.cs";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.ReadSourceView;

            partial record ReadSourceView
            {
                partial record Result
                    : global::Mu.Serialization.IBinder
                {
                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        var meta = model.Add(typeof(ReadSourceView.Result), false);

                        meta.UseConstructor = false;

                        meta.Add(4, "Value");

                        return model;
                    }
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateViewSources(resultDeclaration, string.Empty, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(binder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    [Arguments("sealed partial class", "partial class")]
    [Arguments("sealed partial record", "partial record")]
    [Arguments("readonly partial record struct", "readonly partial record struct")]
    public async Task GivenANestedOutputViewThenItsBinderPreservesTheContainingType(string declaration, string expectedDeclaration)
    {
        // Arrange
        const string resultDeclaration = "public sealed partial record Result(Envelope.View View);";
        const string expectedHint = "MooVC.Testing.Mechanics.Car.ReadSourceView.Envelope.View.Binder.g.cs";
        string referenceDeclaration = $$"""
            public {{declaration}} Envelope
            {
                public sealed partial record View(string Value);
            }
            """;
        string expected = $$"""
            namespace MooVC.Testing.Mechanics.Car.ReadSourceView;

            {{expectedDeclaration}} Envelope
            {
                partial record View
                    : global::Mu.Serialization.IBinder
                {
                    public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                    {
                        var meta = model.Add(typeof(Envelope.View), false);

                        meta.UseConstructor = false;

                        meta.Add(4, "Value");

                        return model;
                    }
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateViewSources(resultDeclaration, referenceDeclaration, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(binder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAReadonlyRecordStructViewThenItsBinderPreservesTheDeclaration()
    {
        // Arrange
        const string resultDeclaration = "public sealed partial record Result(View View);";
        const string referenceDeclaration = "public readonly partial record struct View(string Value);";
        const string expectedHint = "MooVC.Testing.Mechanics.Car.ReadSourceView.View.Binder.g.cs";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.ReadSourceView;

            readonly partial record struct View
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(View), false);

                    meta.UseConstructor = false;

                    meta.Add(4, "Value");

                    return model;
                }
            }
            """;

        // Act
        GeneratorRunResult result = GenerateViewSources(resultDeclaration, referenceDeclaration, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult binder = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(binder.SourceText.ToString()).IsEqualTo(expected);
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenCustomResultAndViewBindersThenTheirBinderDefinitionsAreNotGenerated()
    {
        // Arrange
        const string resultDeclaration = """
            public sealed partial record Result(View View) : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    return model;
                }
            }
            """;
        const string referenceDeclaration = """
            public sealed partial record View(string Value) : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    return model;
                }
            }
            """;
        string[] suppressedHints =
        [
            "MooVC.Testing.Mechanics.Car.ReadSourceView.ReadSourceView.Result.Binder.g.cs",
            "MooVC.Testing.Mechanics.Car.ReadSourceView.View.Binder.g.cs",
        ];

        // Act
        GeneratorRunResult result = GenerateViewSources(resultDeclaration, referenceDeclaration, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Where(definition => suppressedHints.Contains(definition.HintName))).IsEmpty();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenANestedPartialViewWithANonPartialContainerThenItsBinderIsNotGenerated()
    {
        // Arrange
        const string resultDeclaration = "public sealed partial record Result(Envelope.View View);";
        const string suppressedHint = "MooVC.Testing.Mechanics.Car.ReadSourceView.Envelope.View.Binder.g.cs";
        const string referenceDeclaration = """
            public sealed class Envelope
            {
                public sealed partial record View(string Value);
            }
            """;

        // Act
        GeneratorRunResult result = GenerateViewSources(resultDeclaration, referenceDeclaration, out Compilation generated);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == suppressedHint)).IsEmpty();
        _ = await Assert.That(generated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    private static GeneratorRunResult GenerateViewSources(string resultDeclaration, string referenceDeclaration, out Compilation generated)
    {
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed record Car : Mu.Modelling.State.Aggregate;
            }

            namespace {{ViewSourceAssemblyName}}
            {
                public sealed partial record ReadSourceView : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>
                {
                    public ReadSourceView()
                    {
                    }

                    {{resultDeclaration}}
                }

                {{referenceDeclaration}}
            }
            """;
        var compilation = CSharpCompilation.Create(
            ViewSourceAssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out generated, out _);

        return driver.GetRunResult().Results.Single();
    }
}