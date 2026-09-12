namespace Muify.ModelGeneratorTests;

using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed partial class WhenInitializeIsCalled
{
    [Test]
    [Arguments("Creational", false)]
    [Arguments("Creational", true)]
    [Arguments("Transitional", false)]
    [Arguments("Transitional", true)]
    [Arguments("Query", false)]
    [Arguments("Query", true)]
    public async Task GivenAFeatureWithoutConstructorsThenGeneratedConstructorsCompileAndPreserveTheirState(string kind, bool generateBase)
    {
        // Arrange
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Register";
        const string expectedHint = "Register.ctor.g.cs";
        const string expectedBaseHint = "Register.g.cs";
        const string identityText = "1bb066c1-b4fc-417e-a1cf-f5ae50ecf30e";
        const string proposedText = "2026-09-12T12:34:56+00:00";
        const string targetText = "66b7d3bd-8c39-4df0-bd52-1e9a10c53579";
        const string expectedOwner = "Owner";
        const int expectedCount = 3;
        const string expectedDefaultOwner = "Unspecified";
        const int expectedDefaultCount = 1;

        bool isTransitional = kind == "Transitional";
        string arguments = isTransitional ? "Car, System.Guid" : "Car";
        string inheritance = generateBase ? string.Empty : $": {kind}<{arguments}>";
        string attribute = kind == "Query" || !generateBase ? string.Empty : $"[{kind}<Registered>]";
        string unitAttribute = generateBase ? "[Unit<System.Guid>]" : string.Empty;

        string source = $$"""
            namespace Mu.Modelling.Behavior
            {
                using System;
                using Mu.Modelling.State;

                public abstract record UseCase
                {
                    protected UseCase()
                        : this(Guid.NewGuid(), DateTimeOffset.UtcNow)
                    {
                    }

                    protected UseCase(Guid identity, DateTimeOffset proposed)
                    {
                        Identity = identity;
                        Proposed = proposed;
                    }

                    public Guid Identity { get; }

                    public DateTimeOffset Proposed { get; }
                }

                public abstract record Creational<TAggregate> : UseCase
                {
                    protected Creational()
                    {
                    }

                    protected Creational(Guid identity, DateTimeOffset proposed)
                        : base(identity, proposed)
                    {
                    }
                }

                public abstract record Transitional<TAggregate, TIdentity> : UseCase
                    where TIdentity : struct
                {
                    protected Transitional(Reference<TIdentity> target)
                    {
                        Target = target;
                    }

                    protected Transitional(Guid identity, DateTimeOffset proposed, Reference<TIdentity> target)
                        : base(identity, proposed)
                    {
                        Target = target;
                    }

                    public Reference<TIdentity> Target { get; }
                }

                public abstract record Query<TAggregate> : UseCase
                {
                    protected Query()
                    {
                    }

                    protected Query(Guid identity, DateTimeOffset proposed)
                        : base(identity, proposed)
                    {
                    }
                }
            }

            namespace Mu.Modelling.State
            {
                public readonly record struct Reference<TIdentity>(TIdentity Identity)
                    where TIdentity : struct;
            }

            namespace Muify.Service
            {
                public sealed class CreationalAttribute<TFact> : System.Attribute;

                public sealed class TransitionalAttribute<TFact> : System.Attribute;
            }

            namespace MooVC.Testing.Mechanics.Car
            {
                using Muify.Domain;

                {{unitAttribute}}
                public sealed record Car;

                public sealed record Owner(string Name);
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                using Mu.Modelling.Behavior;
                using Muify.Service;

                {{attribute}}
                public sealed partial record Register {{inheritance}}
                {
                    public Owner Owner { get; init; } = new Owner("{{expectedDefaultOwner}}");

                    public int Count { get; init; } = {{expectedDefaultCount}};
                }

                public sealed record Registered;

                public static class FeatureFactory
                {
                    public static Register Create() => Create<Register>();

                    private static TFeature Create<TFeature>()
                        where TFeature : new()
                    {
                        return new TFeature();
                    }
                }
            }
            """;

        string json = $$"""
            {
                "Identity": "{{identityText}}",
                "Proposed": "{{proposedText}}",
                "Owner": { "Name": "{{expectedOwner}}" },
                "Count": {{expectedCount}},
                "Target": { "Identity": "{{targetText}}" }
            }
            """;

        var compilation = CSharpCompilation.Create(
            featureAssemblyName,
            [CSharpSyntaxTree.ParseText(ContractsSource), CSharpSyntaxTree.ParseText(source)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ModelGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        GeneratedSourceResult constructor = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedHint)).HasSingleItem();
        Compilation generatedCompilation = compilation.AddSyntaxTrees(constructor.SyntaxTree);

        if (generateBase)
        {
            GeneratedSourceResult baseDefinition = await Assert.That(result.GeneratedSources.Where(definition => definition.HintName == expectedBaseHint)).HasSingleItem();
            generatedCompilation = generatedCompilation.AddSyntaxTrees(baseDefinition.SyntaxTree);
        }

        _ = await Assert.That(generatedCompilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();

        using var stream = new MemoryStream();
        _ = await Assert.That(generatedCompilation.Emit(stream).Success).IsTrue();
        Assembly assembly = Assembly.Load(stream.ToArray());
        Type requestType = assembly.GetType($"{featureAssemblyName}.Register", throwOnError: true)!;
        Type factoryType = assembly.GetType($"{featureAssemblyName}.FeatureFactory", throwOnError: true)!;
        object defaultRequest = factoryType.GetMethod("Create")!.Invoke(null, null)!;

        _ = await Assert.That((Guid)requestType.GetProperty("Identity")!.GetValue(defaultRequest)!).IsNotEqualTo(Guid.Empty);
        _ = await Assert.That((DateTimeOffset)requestType.GetProperty("Proposed")!.GetValue(defaultRequest)!).IsNotEqualTo(DateTimeOffset.MinValue);
        _ = await Assert.That((int)requestType.GetProperty("Count")!.GetValue(defaultRequest)!).IsEqualTo(expectedDefaultCount);
        object defaultOwner = requestType.GetProperty("Owner")!.GetValue(defaultRequest)!;
        _ = await Assert.That((string)defaultOwner.GetType().GetProperty("Name")!.GetValue(defaultOwner)!).IsEqualTo(expectedDefaultOwner);

        object request = JsonSerializer.Deserialize(json, requestType)!;

        _ = await Assert.That(request).IsNotNull();
        _ = await Assert.That((Guid)requestType.GetProperty("Identity")!.GetValue(request)!).IsEqualTo(Guid.Parse(identityText));
        _ = await Assert.That((DateTimeOffset)requestType.GetProperty("Proposed")!.GetValue(request)!).IsEqualTo(DateTimeOffset.Parse(proposedText, CultureInfo.InvariantCulture));
        _ = await Assert.That((int)requestType.GetProperty("Count")!.GetValue(request)!).IsEqualTo(expectedCount);
        object owner = requestType.GetProperty("Owner")!.GetValue(request)!;
        _ = await Assert.That((string)owner.GetType().GetProperty("Name")!.GetValue(owner)!).IsEqualTo(expectedOwner);

        if (isTransitional)
        {
            object defaultTarget = requestType.GetProperty("Target")!.GetValue(defaultRequest)!;
            _ = await Assert.That((Guid)defaultTarget.GetType().GetProperty("Identity")!.GetValue(defaultTarget)!).IsEqualTo(Guid.Empty);
            object target = requestType.GetProperty("Target")!.GetValue(request)!;
            _ = await Assert.That((Guid)target.GetType().GetProperty("Identity")!.GetValue(target)!).IsEqualTo(Guid.Parse(targetText));
        }
    }
}