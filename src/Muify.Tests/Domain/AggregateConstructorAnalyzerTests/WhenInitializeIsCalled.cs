namespace Muify.Domain.AggregateConstructorAnalyzerTests;

using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

public sealed class WhenInitializeIsCalled
{
    private const string ContractsSource = """
        namespace Mu.Modelling.State
        {
            public abstract record Aggregate;
        }

        namespace Muify.Domain
        {
            using System;

            [AttributeUsage(AttributeTargets.Class, Inherited = false)]
            public sealed class UnitAttribute<TIdentity> : Attribute
                where TIdentity : struct
            {
            }
        }
        """;

    private const string ExpectedMessage = "Type `Wheel` deriving from Aggregate or annotated with Unit<> must satisfy the new() constraint: it must be non-abstract and have a public parameterless constructor that satisfies any required members";
    private const string ExpectedTypeName = "Wheel";

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    [Arguments("public sealed record Wheel(int Size) : Aggregate;")]
    [Arguments("public sealed record Wheel(int Size = 0) : Aggregate;")]
    [Arguments("public sealed record Wheel(params int[] Sizes) : Aggregate;")]
    [Arguments("[Unit<int>] public sealed partial record Wheel(int Size);")]
    [Arguments("[Unit<int>] public sealed record Wheel(int Size) : Aggregate;")]
    [Arguments("public abstract record Wheel(int Size) : Aggregate { public Wheel() : this(0) { } }")]
    [Arguments("[Unit<int>] public abstract record Wheel(int Size) { public Wheel() : this(0) { } }")]
    public async Task GivenAPositionalRecordThatCannotSatisfyTheConstructorConstraintThenAnErrorShouldBeReturned(string declaration)
    {
        // Arrange
        string source = $$"""
            namespace Testing.Wheel;

            using Mu.Modelling.State;
            using Muify.Domain;

            {{declaration}}
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(AggregateConstructorAnalyzer.ConstructorConstraintNotSatisfiedId);
        _ = await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(ExpectedMessage);
        _ = await Assert.That(source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length)).IsEqualTo(ExpectedTypeName);
    }

    [Test]
    [Arguments("internal")]
    [Arguments("private")]
    [Arguments("protected")]
    [Arguments("protected internal")]
    [Arguments("private protected")]
    public async Task GivenAPositionalAggregateWithANonPublicParameterlessConstructorThenAnErrorShouldBeReturned(string accessibility)
    {
        // Arrange
        string source = $$"""
            namespace Testing.Wheel;

            using Mu.Modelling.State;

            public record Wheel(int Size) : Aggregate
            {
                {{accessibility}} Wheel() : this(0)
                {
                }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.Id).IsEqualTo(AggregateConstructorAnalyzer.ConstructorConstraintNotSatisfiedId);
    }

    [Test]
    [Arguments("[Unit<int>] public sealed record Wheel(int Size) { public Wheel() : this(0) { } }")]
    [Arguments("public sealed record Wheel(int Size) : Aggregate { public Wheel() : this(0) { } }")]
    [Arguments("public sealed record Wheel : Aggregate;")]
    [Arguments("public sealed record Wheel() : Aggregate;")]
    [Arguments("[Unit<int>] public sealed record Wheel;")]
    [Arguments("public sealed record Wheel(int Size);")]
    [Arguments("public sealed record Wheel(int Size) : Unrelated.Aggregate;")]
    [Arguments("[Unrelated.Unit<int>] public sealed record Wheel(int Size);")]
    [Arguments("[Unit<int>] public sealed class Wheel(int size);")]
    public async Task GivenAValidOrUnrelatedTypeThenNoDiagnosticShouldBeReturned(string declaration)
    {
        // Arrange
        string source = $$"""
            namespace Testing.Wheel
            {
                using Mu.Modelling.State;
                using Muify.Domain;

                {{declaration}}
            }

            namespace Unrelated
            {
                public abstract record Aggregate;

                public sealed class UnitAttribute<TIdentity> : System.Attribute;
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAnIndirectPositionalAggregateThenAnErrorShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            using AggregateBase = Mu.Modelling.State.Aggregate;

            public abstract record Vehicle : AggregateBase;

            public sealed record Wheel(int Size) : Vehicle;
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(ExpectedMessage);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAUnitAttributeOnAnotherPartialDeclarationThenAnErrorShouldBeReturned(bool hasGeneratedBase)
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            public sealed partial record Wheel(int Size);
            """;
        const string attributeSource = """
            namespace Testing.Wheel;

            using Unit = Muify.Domain.UnitAttribute<int>;

            [Unit]
            public sealed partial record Wheel;
            """;
        string baseSource = hasGeneratedBase
            ? "namespace Testing.Wheel; public sealed partial record Wheel : Mu.Modelling.State.Aggregate;"
            : string.Empty;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source, attributeSource, baseSource);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(ExpectedMessage);
        _ = await Assert.That(diagnostic.Location.SourceTree?.ToString()).IsEqualTo(source);
    }

    [Test]
    public async Task GivenAParameterlessConstructorOnAnotherPartialDeclarationThenNoDiagnosticShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Wheel(int Size);
            """;
        const string constructorSource = """
            namespace Testing.Wheel;

            public sealed partial record Wheel
            {
                public Wheel() : this(0)
                {
                }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source, constructorSource);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    [Arguments("public required string Name { get; init; }")]
    [Arguments("public required string Name;")]
    public async Task GivenAPositionalAggregateWithRequiredMembersThenAnErrorShouldBeReturned(string member)
    {
        // Arrange
        string source = $$"""
            namespace Testing.Wheel;

            using Mu.Modelling.State;

            public sealed record Wheel(int Size) : Aggregate
            {
                public Wheel() : this(0)
                {
                }

                {{member}}
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(ExpectedMessage);
    }

    [Test]
    public async Task GivenAPositionalAggregateWithInheritedRequiredMembersThenAnErrorShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            using Mu.Modelling.State;

            public abstract record Vehicle : Aggregate
            {
                public required string Name { get; init; }
            }

            public sealed record Wheel(int Size) : Vehicle
            {
                public Wheel() : this(0)
                {
                }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        Diagnostic diagnostic = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(diagnostic.GetMessage()).IsEqualTo(ExpectedMessage);
    }

    [Test]
    public async Task GivenAParameterlessConstructorThatSetsRequiredMembersThenNoDiagnosticShouldBeReturned()
    {
        // Arrange
        const string source = """
            namespace Testing.Wheel;

            using System.Diagnostics.CodeAnalysis;
            using Mu.Modelling.State;

            public sealed record Wheel(int Size) : Aggregate
            {
                [SetsRequiredMembers]
                public Wheel() : this(0)
                {
                    Name = string.Empty;
                }

                public required string Name { get; init; }
            }
            """;

        // Act
        ImmutableArray<Diagnostic> result = await GetDiagnostics(source);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnostics(params string[] sources)
    {
        var compilation = CSharpCompilation.Create(
            "Testing",
            sources.Prepend(ContractsSource).Select(source => CSharpSyntaxTree.ParseText(source)),
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        _ = await Assert.That(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        var analyzer = new AggregateConstructorAnalyzer();
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