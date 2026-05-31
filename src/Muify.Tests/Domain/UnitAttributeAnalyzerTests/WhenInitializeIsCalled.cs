namespace Muify.Domain.UnitAttributeAnalyzerTests;

using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

public sealed class WhenInitializeIsCalled
{
    private const string AttributeSource = """
        namespace Muify.Domain;

        using System;

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
        public sealed class UnitAttribute<TIdentity>
            : Attribute
            where TIdentity : struct
        {
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenARecordWithMatchingNamespaceThenNoDiagnosticShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            using Muify.Domain;

            [Unit<int>]
            public sealed record Wheel;
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAClassWithAUnitAttributeThenTypeNotSupportedDiagnosticShouldBeReturned()
    {
        // Arrange
        const string expectedMessage = "Unit can only be used on records";

        const string source = """
            namespace Testing.Wheel;

            using Muify.Domain;

            [Unit<int>]
            public sealed class Wheel
            {
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(UnitAttributeAnalyzer.TypeNotSupportedId);
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(expectedMessage);
    }

    [Test]
    public async Task GivenARecordWhoseNameDoesNotMatchTheNamespaceThenTypeNameMismatchDiagnosticShouldBeReturned()
    {
        // Arrange
        const string expectedMessage = "Unit type `Wheel` must match the last segment of its namespace";

        const string source = """
            namespace Testing.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed record Wheel;
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(UnitAttributeAnalyzer.TypeNameMismatchId);
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(expectedMessage);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Testing",
            [
                CSharpSyntaxTree.ParseText(AttributeSource),
                CSharpSyntaxTree.ParseText(source),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new UnitAttributeAnalyzer();
        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);
        CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static MetadataReference[] GetReferences()
    {
        string trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}