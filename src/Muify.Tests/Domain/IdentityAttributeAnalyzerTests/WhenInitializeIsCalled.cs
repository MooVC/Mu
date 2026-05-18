namespace Muify.Domain.IdentityAttributeAnalyzerTests;

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
        public sealed class IdentityAttribute
            : Attribute
        {
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenAClassWithOneIdentityPropertyThenNoDiagnosticShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing;

            using Muify.Domain;

            public sealed class Wheel
            {
                [Identity]
                public int Id { get; set; }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAClassWithTwoIdentityPropertiesThenDuplicateIdentityAttributeDiagnosticsShouldBeReturned()
    {
        // Arrange
        const string expectedMessage = "Type `Wheel` has multiple identity properties";

        const string source = """
            namespace Testing;

            using Muify.Domain;

            public sealed class Wheel
            {
                [Identity]
                public int Id { get; set; }

                [Identity]
                public int Number { get; set; }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        _ = await Assert.That(result.Length).IsEqualTo(2);
        _ = await Assert.That(result[0].Id).IsEqualTo(IdentityAttributeAnalyzer.DuplicateIdentityAttributeId);
        _ = await Assert.That(result[0].GetMessage()).IsEqualTo(expectedMessage);
        _ = await Assert.That(result[1].Id).IsEqualTo(IdentityAttributeAnalyzer.DuplicateIdentityAttributeId);
        _ = await Assert.That(result[1].GetMessage()).IsEqualTo(expectedMessage);
    }

    [Test]
    public async Task GivenARecordWithAnIdentityPropertyThenTypeNotSupportedDiagnosticShouldBeReturned()
    {
        // Arrange
        const string expectedMessage = "Identity can only be used on properties declared by a class";

        const string source = """
            namespace Testing;

            using Muify.Domain;

            public sealed record Wheel
            {
                [Identity]
                public int Id { get; init; }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(IdentityAttributeAnalyzer.TypeNotSupportedId);
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(expectedMessage);
    }

    [Test]
    public async Task GivenAStructWithAnIdentityPropertyThenTypeNotSupportedDiagnosticShouldBeReturned()
    {
        // Arrange
        const string expectedMessage = "Identity can only be used on properties declared by a class";

        const string source = """
            namespace Testing;

            using Muify.Domain;

            public struct Wheel
            {
                [Identity]
                public int Id { get; set; }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(IdentityAttributeAnalyzer.TypeNotSupportedId);
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

        var analyzer = new IdentityAttributeAnalyzer();
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