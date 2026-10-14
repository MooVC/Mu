namespace Muify.Semantics.CompilationExtensionTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseModelIsCalled
{
    private const string ClassificationAttributesSource = """
        namespace Muify.Service
        {
            [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
            public sealed class CreationalAttribute<TFact> : global::System.Attribute;

            [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
            public sealed class NonMutationalAttribute : global::System.Attribute;

            [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
            public sealed class TransitionalAttribute<TFact> : global::System.Attribute;
        }
        """;

    [Test]
    [Arguments("", false)]
    [Arguments("", true)]
    [Arguments("Platform", false)]
    [Arguments("Platform", true)]
    [Arguments("Platform.Payments", false)]
    [Arguments("Platform.Payments", true)]
    public async Task GivenAUnitAtAnySupportedDepthWhenAnnotatedOrDerivedFromAggregateThenItsScopeAndNamespacesArePreserved(string scope, bool hasAttribute)
    {
        // Arrange
        const string unitName = "Car";
        string areaNamespace = GetScopedAreaNamespace(scope);
        string unitNamespace = $"{areaNamespace}.{unitName}";
        string attribute = hasAttribute ? "[global::Muify.Domain.Unit<global::System.Guid>]" : string.Empty;
        string inheritance = hasAttribute ? string.Empty : ": IntermediateAggregate";
        string source = $$"""
            namespace {{unitNamespace}};

            public abstract record IntermediateAggregate : global::Mu.Modelling.State.Aggregate;

            {{attribute}}
            public sealed partial record Car {{inheritance}};
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(unitNamespace, source)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(AttributeSource));

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Company.ToString()).IsEqualTo("MooVC");
        _ = await Assert.That(result.Name.ToString()).IsEqualTo("Testing");
        _ = await Assert.That(result.Scope.ToString()).IsEqualTo(scope);
        _ = await Assert.That(result.Metadata.IsHost).IsFalse();
        Model.Graph.Areas.Area.Units.Unit unit = await GetScopedUnitGraph(result, areaNamespace, unitNamespace);
        _ = await Assert.That(unit._Value.Features).IsEmpty();
    }

    [Test]
    [Arguments("", "Creational")]
    [Arguments("", "Transitional")]
    [Arguments("", "NonMutational")]
    [Arguments("Platform", "Creational")]
    [Arguments("Platform", "Transitional")]
    [Arguments("Platform", "NonMutational")]
    [Arguments("Platform.Payments", "Creational")]
    [Arguments("Platform.Payments", "Transitional")]
    [Arguments("Platform.Payments", "NonMutational")]
    public async Task GivenAnAnnotatedFeatureAtAnySupportedDepthThenItsKindScopeAndNamespacesArePreserved(string scope, string kind)
    {
        // Arrange
        const string featureName = "Open";
        string areaNamespace = GetScopedAreaNamespace(scope);
        string unitNamespace = $"{areaNamespace}.Car";
        string featureNamespace = $"{unitNamespace}.{featureName}";
        string attribute = kind == "NonMutational" ? "NonMutational" : $"{kind}<Changed>";
        string source = $$"""
            namespace {{unitNamespace}}
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace {{featureNamespace}}
            {
                [global::Muify.Service.{{attribute}}]
                public sealed partial record Open;

                public sealed record Changed;
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(featureNamespace, source);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Company.ToString()).IsEqualTo("MooVC");
        _ = await Assert.That(result.Name.ToString()).IsEqualTo("Testing");
        _ = await Assert.That(result.Scope.ToString()).IsEqualTo(scope);
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(featureNamespace);
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo($"Build{featureName}");
        Model.Graph.Areas.Area.Units.Unit unit = await GetScopedUnitGraph(result, areaNamespace, unitNamespace);
        Feature feature = await Assert.That(unit._Value.Features).HasSingleItem();
        var features = new Model.Graph.Areas.Area.Units.Unit.Features(unit, result, unit._Value.Features);
        var graph = new Model.Graph.Areas.Area.Units.Unit.Features.Feature(features, 0, result, feature);
        _ = await Assert.That(graph.Namespace.ToString()).IsEqualTo(featureNamespace);
        _ = await Assert.That(feature.Name.ToString()).IsEqualTo(featureName);
        _ = await Assert.That(feature.Type.IsNonMutational).IsEqualTo(kind == "NonMutational");

        if (kind != "NonMutational")
        {
            Mutational.Kinds expected = kind == "Creational" ? Mutational.Kinds.Creational : Mutational.Kinds.Transitional;
            _ = await Assert.That(feature.Mutational.Type).IsEqualTo(expected);
        }
    }

    [Test]
    [Arguments("", false)]
    [Arguments("", true)]
    [Arguments("Platform.Payments", false)]
    [Arguments("Platform.Payments", true)]
    public async Task GivenAFeatureDerivedFromCausalWhenItsBaseIsDirectOrIndirectThenItsTerminalNameDeterminesItsNamespaces(string scope, bool isIndirect)
    {
        // Arrange
        string areaNamespace = GetScopedAreaNamespace(scope);
        string unitNamespace = $"{areaNamespace}.Car";
        string featureNamespace = $"{unitNamespace}.Open";
        string inheritance = isIndirect ? "IntermediateRequest" : "global::Mu.Modelling.Behavior.Causal";
        string source = $$"""
            namespace Mu.Modelling.Behavior
            {
                public abstract record Causal;
            }

            namespace {{unitNamespace}}
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace {{featureNamespace}}
            {
                public abstract record IntermediateRequest : global::Mu.Modelling.Behavior.Causal;

                public sealed partial record Open : {{inheritance}};
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(featureNamespace, source);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Name.ToString()).IsEqualTo("Testing");
        _ = await Assert.That(result.Scope.ToString()).IsEqualTo(scope);
        Model.Graph.Areas.Area.Units.Unit unit = await GetScopedUnitGraph(result, areaNamespace, unitNamespace);
        Feature feature = await Assert.That(unit._Value.Features).HasSingleItem();
        _ = await Assert.That(feature.Name.ToString()).IsEqualTo("Open");
        _ = await Assert.That(result.Metadata.Features).IsEquivalentTo(new Qualification[] { (Name: "Open", Qualifier: featureNamespace) });
    }

    [Test]
    [Arguments("", "")]
    [Arguments("", "public sealed class HostMarker;")]
    [Arguments("", "public sealed record Host;")]
    [Arguments("", "public sealed record Incidental;")]
    [Arguments("Platform", "")]
    [Arguments("Platform", "public sealed class HostMarker;")]
    [Arguments("Platform.Payments", "")]
    [Arguments("Platform.Payments", "public sealed class HostMarker;")]
    [Arguments("Platform.Payments", "public sealed record Host;")]
    public async Task GivenAHostSuffixAtAnySupportedDepthThenItsScopeAndApplicationNamespaceArePreservedRegardlessOfDefinedTypes(string scope, string definition)
    {
        // Arrange
        string hostNamespace = $"{GetScopedAreaNamespace(scope)}.Car.Host";
        string source = string.IsNullOrEmpty(definition) ? string.Empty : $"namespace {hostNamespace}; {definition}";
        CSharpCompilation compilation = CreateApplicationCompilation(hostNamespace, source);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Company.ToString()).IsEqualTo("MooVC");
        _ = await Assert.That(result.Name.ToString()).IsEqualTo("Testing");
        _ = await Assert.That(result.Scope.ToString()).IsEqualTo(scope);
        _ = await Assert.That(result.Metadata.IsHost).IsTrue();
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(hostNamespace);
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo("BuildCar");
        _ = await Assert.That(result.Areas).IsEmpty();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAUnitOrFeatureInAnAliasedRootNamespaceThenItsCanonicalScopeAndGraphNamespacesAreUsed(bool isFeature)
    {
        // Arrange
        const string assemblyName = "Custom.Runtime";
        const string areaNamespace = "MooVC.Testing.Platform.Payments.Mechanics";
        const string unitNamespace = $"{areaNamespace}.Car";
        string projectNamespace = isFeature ? $"{unitNamespace}.Open" : unitNamespace;
        const string source = $$"""
            namespace {{unitNamespace}}
            {
                public sealed record Car : global::Mu.Modelling.State.Aggregate;
            }

            namespace {{unitNamespace}}.Open
            {
                [global::Muify.Service.NonMutational]
                public sealed partial record Open;
            }
            """;
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source);

        // Act
        Model result = compilation.ParseModel(projectNamespace, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Company.ToString()).IsEqualTo("MooVC");
        _ = await Assert.That(result.Name.ToString()).IsEqualTo("Testing");
        _ = await Assert.That(result.Scope.ToString()).IsEqualTo("Platform.Payments");
        _ = await Assert.That(result.Metadata.ApplicationNamespace).IsEqualTo(projectNamespace);
        Model.Graph.Areas.Area.Units.Unit unit = await GetScopedUnitGraph(result, areaNamespace, unitNamespace);
        _ = await Assert.That(unit._Value.Features.Length).IsEqualTo(isFeature ? 1 : 0);
    }

    [Test]
    public async Task GivenAMatchingUnitRecordOnlyInAReferencedAssemblyThenItDoesNotClassifyTheCurrentProject()
    {
        // Arrange
        const string referenceAssemblyName = "External.Domain";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car : global::Mu.Modelling.State.Aggregate;
            """;
        MetadataReference reference = await CreateApplicationReference(referenceAssemblyName, source);
        CSharpCompilation compilation = CreateApplicationCompilation(AssemblyName).AddReferences(reference);

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Areas).IsEmpty();
        _ = await Assert.That(result.Metadata.Features).IsEmpty();
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(string.Empty);
    }

    [Test]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace MooVC.Testing.Mechanics.Car; public sealed record Car;")]
    [Arguments("MooVC.Testing.Platform.Payments.Mechanics.Car.Open", "namespace MooVC.Testing.Platform.Payments.Mechanics.Car.Open; public sealed record Open;")]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace MooVC.Testing.Mechanics.Car; [Muify.Domain.Unit<System.Guid>] public sealed class Car;")]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace MooVC.Testing.Mechanics.Car; [Muify.Domain.Unit<System.Guid>] public sealed record Other;")]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace MooVC.Testing.Mechanics.Other; [Muify.Domain.Unit<System.Guid>] public sealed record Car;")]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace Other { public abstract record Aggregate; } namespace MooVC.Testing.Mechanics.Car { public sealed record Car : Other.Aggregate; }")]
    [Arguments("MooVC.Testing.Mechanics.Car", "namespace Other { public sealed class UnitAttribute<TIdentity> : System.Attribute; } namespace MooVC.Testing.Mechanics.Car { [Other.Unit<System.Guid>] public sealed record Car; }")]
    [Arguments("MooVC.Testing.Mechanics.Car.Open", "namespace Other { public abstract record Causal; } namespace MooVC.Testing.Mechanics.Car.Open { public sealed record Open : Other.Causal; }")]
    [Arguments("MooVC.Testing.Mechanics.Car.Open", "namespace Other { public sealed class NonMutationalAttribute : System.Attribute; } namespace MooVC.Testing.Mechanics.Car.Open { [Other.NonMutational] public sealed record Open; }")]
    public async Task GivenAnUnrecognizedOrMismatchedTerminalDefinitionThenNoDomainOrApplicationIsCatalogued(string assemblyName, string source)
    {
        // Arrange
        CSharpCompilation compilation = CreateApplicationCompilation(assemblyName, source)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(AttributeSource));

        // Act
        Model result = compilation.ParseModel(CancellationToken.None);

        // Assert
        _ = await Assert.That(result.Areas).IsEmpty();
        _ = await Assert.That(result.Metadata.IsHost).IsFalse();
        _ = await Assert.That(result.Metadata.BuildName).IsEqualTo(string.Empty);
        _ = await Assert.That(result.Metadata.Features).IsEmpty();
    }

    private static string GetScopedAreaNamespace(string scope)
    {
        return string.IsNullOrEmpty(scope) ? "MooVC.Testing.Mechanics" : $"MooVC.Testing.{scope}.Mechanics";
    }

    private static async Task<Model.Graph.Areas.Area.Units.Unit> GetScopedUnitGraph(Model model, string areaNamespace, string unitNamespace)
    {
        Area area = await Assert.That(model.Areas).HasSingleItem();
        Unit unit = await Assert.That(area.Units).HasSingleItem();
        var areas = new Model.Graph.Areas(model, model.Areas);
        var areaGraph = new Model.Graph.Areas.Area(areas, 0, model, area);
        var units = new Model.Graph.Areas.Area.Units(areaGraph, model, area.Units);
        var unitGraph = new Model.Graph.Areas.Area.Units.Unit(units, 0, model, unit);
        _ = await Assert.That(area.Name.ToString()).IsEqualTo("Mechanics");
        _ = await Assert.That(unit.Name.ToString()).IsEqualTo("Car");
        _ = await Assert.That(areaGraph.Namespace.ToString()).IsEqualTo(areaNamespace);
        _ = await Assert.That(unitGraph.Namespace.ToString()).IsEqualTo(unitNamespace);

        return unitGraph;
    }
}