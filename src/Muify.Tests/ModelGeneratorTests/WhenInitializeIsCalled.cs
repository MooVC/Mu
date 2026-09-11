namespace Muify.ModelGeneratorTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed class WhenInitializeIsCalled
{
    private const string AssemblyName = "MooVC.Testing.Mechanics.Car";

    private const string ContractsSource = """
        namespace Muify.Domain
        {
            using System;

            [AttributeUsage(AttributeTargets.Class)]
            public sealed class UnitAttribute<TIdentity> : Attribute
                where TIdentity : struct
            {
            }
        }

        namespace Mu.Modelling.Services
        {
            public interface IAllocator<TIdentity>
                where TIdentity : struct
            {
            }
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenACreationalFeatureWithAReferencedUnitThenItsFactAndTransformIncludeThePayloadInTheFeatureNamespace()
    {
        // Arrange
        const string domainAssemblyName = "MooVC.Testing.Modelling.Account";
        const string featureAssemblyName = "MooVC.Testing.Modelling.Account.Open";
        const string expectedBaseHint = "Open.g.cs";
        const string expectedBase = """
            namespace MooVC.Testing.Modelling.Account.Open;

            public sealed partial record Open
                : global::Mu.Modelling.Behavior.Creational<global::MooVC.Testing.Modelling.Account.Account>;
            """;
        const string expectedHint = "Opened.g.cs";
        const string expectedNamespace = "namespace MooVC.Testing.Modelling.Account.Open;";
        const string expectedPayload = "global::MooVC.Testing.Modelling.Account.Owner Owner";
        const string expectedTransformHint = "Transform.g.cs";
        const string expectedTransform = """
            namespace MooVC.Testing.Modelling.Account.Open;

            using MooVC.Testing.Modelling.Account;

            internal sealed class Transform
                : global::Mu.Modelling.Services.ITransform<global::MooVC.Testing.Modelling.Account.Account, global::MooVC.Testing.Modelling.Account.Open.Opened>
            {
                public global::MooVC.Testing.Modelling.Account.Account Apply(
                    global::MooVC.Testing.Modelling.Account.Account aggregate,
                    global::MooVC.Testing.Modelling.Account.Open.Opened fact)
                {
                    return aggregate with
                    {
                        Owner = fact.Owner,
                    };
                }
            }
            """;
        const string domainSource = """
            namespace MooVC.Testing.Modelling.Account;

            public sealed record Account(Owner Owner);

            public sealed record Owner(string Name);
            """;
        const string featureSource = """
            namespace Mu.Modelling.Behavior
            {
                public abstract record Creational<TAggregate>;

                public abstract record Fact<TAggregate>
                {
                    protected Fact()
                    {
                    }

                    protected Fact(System.Guid identity, System.DateTimeOffset proposed)
                    {
                    }
                }

                public interface IConvertFrom<out TSelf, in TSource>
                    where TSelf : IConvertFrom<TSelf, TSource>
                {
                    static abstract implicit operator TSelf(TSource subject);
                }
            }

            namespace Mu.Modelling.Services
            {
                public interface ITransform<TAggregate, TFact>
                {
                    TAggregate Apply(TAggregate aggregate, TFact fact);
                }
            }

            namespace Muify.Service
            {
                using System;

                [AttributeUsage(AttributeTargets.Class)]
                public sealed class CreationalAttribute<TFact> : Attribute
                {
                }
            }

            namespace MooVC.Testing.Modelling.Account.Open
            {
                using Muify.Service;

                [Creational<Opened>]
                public sealed partial record Open(Owner Owner);
            }
            """;

        var domain = CSharpCompilation.Create(
            domainAssemblyName,
            [CSharpSyntaxTree.ParseText(domainSource)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        _ = domain.Emit(stream);
        stream.Position = 0;

        var compilation = CSharpCompilation.Create(
            featureAssemblyName,
            [CSharpSyntaxTree.ParseText(featureSource)],
            _references.Append(MetadataReference.CreateFromStream(stream)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult baseDefinition = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedBaseHint)).HasSingleItem();
        _ = await Assert.That(baseDefinition.SourceText.ToString()).IsEqualTo(expectedBase);
        GeneratedSourceResult fact = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        _ = await Assert.That(fact.SourceText.ToString()).Contains(expectedNamespace);
        _ = await Assert.That(fact.SourceText.ToString()).Contains(expectedPayload);
        GeneratedSourceResult transform = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedTransformHint)).HasSingleItem();
        _ = await Assert.That(transform.SourceText.ToString()).IsEqualTo(expectedTransform);
        Compilation generatedCompilation = compilation.AddSyntaxTrees(baseDefinition.SyntaxTree, fact.SyntaxTree, transform.SyntaxTree);
        _ = await Assert.That(generatedCompilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
    }

    [Test]
    public async Task GivenAPartialAllocatorWithoutARegistrarThenAllocatorRegistrarIsGenerated()
    {
        // Arrange
        const string expectedHint = "Allocator.Registrar.g.cs";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Mu.Modelling.Services;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;

            public sealed partial class Allocator : IAllocator<Guid>
            {
            }
            """;

        // Act
        GeneratorRunResult result = Generate(source);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Select(definition => definition.HintName)).Contains(expectedHint);
    }

    [Test]
    public async Task GivenAPartialUnitWithoutABinderThenBinderIsGenerated()
    {
        // Arrange
        const string expectedHint = "Car.Binder.g.cs";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;
            """;

        // Act
        GeneratorRunResult result = Generate(source);

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        _ = await Assert.That(result.GeneratedSources.Select(definition => definition.HintName)).Contains(expectedHint);
    }

    private static GeneratorRunResult Generate(string source)
    {
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(ContractsSource),
                CSharpSyntaxTree.ParseText(source),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        return driver.RunGenerators(compilation).GetRunResult().Results.Single();
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