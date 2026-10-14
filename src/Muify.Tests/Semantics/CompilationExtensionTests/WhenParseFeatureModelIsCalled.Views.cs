namespace Muify.Semantics.CompilationExtensionTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseFeatureModelIsCalled
{
    [Test]
    [Arguments("public sealed partial record Result;", 0)]
    [Arguments("public sealed partial record Result(string Value);", 1)]
    public async Task GivenANestedResultThenItsBinderMetadataIsCataloguedEvenWithoutProperties(string declaration, int attributeCount)
    {
        // Arrange
        const string expectedName = "Register.Result";
        const string expectedContainer = "Register";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                {{declaration}}
            }
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single();

        // Assert
        _ = await Assert.That(result.Qualification.Moniker.ToString()).IsEqualTo(expectedName);
        _ = await Assert.That(result.Qualification.Qualifier.ToString()).IsEqualTo(AssemblyName);
        _ = await Assert.That(result.Attributes.Length).IsEqualTo(attributeCount);
        _ = await Assert.That(result.ContainingTypes.Single().Qualification.Moniker.ToString()).IsEqualTo(expectedContainer);
        _ = await Assert.That(result.Characteristics.IsRecord).IsTrue();
        _ = await Assert.That(result.IsAccessible).IsTrue();
        _ = await Assert.That(result.IsPartial).IsTrue();
        _ = await Assert.That(result.HasBinder).IsFalse();
    }

    [Test]
    [Arguments("partial", "partial", true)]
    [Arguments("partial", "", false)]
    [Arguments("", "partial", false)]
    [Arguments("", "", false)]
    public async Task GivenANestedResultThenEveryContainingDeclarationMustBePartial(string requestModifier, string resultModifier, bool isPartial)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed {{requestModifier}} record Register
            {
                public sealed {{resultModifier}} record Result(string Value);
            }
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single();

        // Assert
        _ = await Assert.That(result.IsPartial).IsEqualTo(isPartial);
        _ = await Assert.That(result.IsAccessible).IsTrue();
    }

    [Test]
    public async Task GivenOutputCollectionsAndCyclesThenViewsOutsideTheFeatureNamespaceAreDiscoveredOnce()
    {
        // Arrange
        const string contractsNamespace = "MooVC.Testing.Contracts";
        const string source = """
            #nullable enable

            namespace MooVC.Testing.Contracts
            {
                public sealed partial record View(Detail[] Details);

                public sealed partial record Detail(View? Parent, Token? Token);

                public readonly partial record struct Token(string Value);
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed partial record Register
                {
                    public sealed partial record Result(System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.List<MooVC.Testing.Contracts.View[]>> Views);
                }
            }
            """;
        Qualification[] expected =
        [
            (Name: "Register.Result", Qualifier: AssemblyName),
            (Name: "View", Qualifier: contractsNamespace),
            (Name: "Detail", Qualifier: contractsNamespace),
            (Name: "Token", Qualifier: contractsNamespace),
        ];

        // Act
        IEnumerable<Poco> result = GetFeature(source).Metadata.References;

        // Assert
        _ = await Assert.That(result.Select(reference => reference.Qualification)).IsEquivalentTo(expected);
        _ = await Assert.That(result.All(reference => reference.IsAccessible && reference.IsPartial)).IsTrue();
    }

    [Test]
    public async Task GivenAGenericOutputTypeThenItsNonGenericArgumentsAreCataloguedWithoutTheGenericDeclaration()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(View<Details> View);
            }

            public sealed partial record View<TValue>(TValue Value);

            public sealed partial record Details(string Value);
            """;
        Qualification[] expected =
        [
            (Name: "Register.Result", Qualifier: AssemblyName),
            (Name: "Details", Qualifier: AssemblyName),
        ];

        // Act
        IEnumerable<Qualification> result = GetFeature(source).Metadata.References.Select(reference => reference.Qualification);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }

    [Test]
    public async Task GivenAViewSharedByRequestAndResultThenItIsCataloguedOnceWithoutRecataloguingTheRequest()
    {
        // Arrange
        const string source = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register(View View)
            {
                public sealed partial record Result(View View);
            }

            public sealed partial record View(Register? Request);
            """;
        Qualification[] expected =
        [
            (Name: "View", Qualifier: AssemblyName),
            (Name: "Register.Result", Qualifier: AssemblyName),
        ];

        // Act
        IEnumerable<Qualification> result = GetFeature(source).Metadata.References.Select(reference => reference.Qualification);

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }

    [Test]
    public async Task GivenResultPropertiesThenOnlyPublicInstanceSettablePropertiesAreCatalogued()
    {
        // Arrange
        const string expectedName = "Value";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(string Value)
                {
                    public static int Count { get; set; }

                    public string Description => Value;

                    private int State { get; set; }

                    public string this[int index]
                    {
                        get => Value;
                        set { }
                    }
                }
            }
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single();

        // Assert
        _ = await Assert.That(result.Attributes.Single().Name.ToString()).IsEqualTo(expectedName);
    }

    [Test]
    public async Task GivenANestedViewThenItsContainingDeclarationsAreCataloguedFromInnermostToOutermost()
    {
        // Arrange
        const string expectedName = "Envelope.Branch.View";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(Envelope.Branch.View View);
            }

            public static partial class Envelope
            {
                public partial class Branch
                {
                    public sealed partial record View(string Value);
                }
            }
            """;
        string[] expectedContainers = ["Envelope.Branch", "Envelope"];

        // Act
        Poco result = GetFeature(source).Metadata.References.Single(reference => reference.Qualification.Moniker.ToString() == expectedName);

        // Assert
        _ = await Assert.That(result.ContainingTypes.Select(container => container.Qualification.Moniker.ToString()).SequenceEqual(expectedContainers)).IsTrue();
        _ = await Assert.That(result.ContainingTypes.All(container => container.Characteristics.IsClass)).IsTrue();
        _ = await Assert.That(result.IsAccessible).IsTrue();
        _ = await Assert.That(result.IsPartial).IsTrue();
    }

    [Test]
    [Arguments("public sealed partial record View(string Value);", "View", true, true)]
    [Arguments("public sealed record View(string Value);", "View", true, false)]
    [Arguments("internal sealed partial record View(string Value);", "View", true, true)]
    [Arguments("file sealed partial record View(string Value);", "View", false, true)]
    [Arguments("public static class Envelope { public sealed partial record View(string Value); }", "Envelope.View", true, false)]
    [Arguments("public static partial class Envelope<T> { public sealed partial record View(string Value); }", "Envelope<int>.View", false, true)]
    public async Task GivenViewDeclarationsThenAccessibilityAndPartialAncestryAreCatalogued(string declaration, string typeName, bool isAccessible, bool isPartial)
    {
        // Arrange
        const string resultName = "Register.Result";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result({{typeName}} View);
            }

            {{declaration}}
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single(reference => reference.Qualification.Moniker.ToString() != resultName);

        // Assert
        _ = await Assert.That(result.IsAccessible).IsEqualTo(isAccessible);
        _ = await Assert.That(result.IsPartial).IsEqualTo(isPartial);
    }

    [Test]
    public async Task GivenAnInaccessibleOutputTypeThenItsBinderMetadataIsNotCatalogued()
    {
        // Arrange
        const string expectedName = "Register.Result";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(Envelope.View View);
            }

            public static partial class Envelope
            {
                private sealed partial record View(string Value);
            }
            """;

        // Act
        IEnumerable<Poco> result = GetFeature(source).Metadata.References;

        // Assert
        Poco reference = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(reference.Qualification.Moniker.ToString()).IsEqualTo(expectedName);
    }

    [Test]
    public async Task GivenCustomResultAndViewBindersThenTheyAreDetectedWithoutPartialDeclarations()
    {
        // Arrange
        const string source = """
            namespace Mu.Serialization
            {
                public interface IBinder;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed partial record Register
                {
                    public sealed record Result(View View) : Mu.Serialization.IBinder;
                }

                public sealed record View(string Value) : Mu.Serialization.IBinder;
            }
            """;
        Qualification[] expected =
        [
            (Name: "Register.Result", Qualifier: AssemblyName),
            (Name: "View", Qualifier: AssemblyName),
        ];

        // Act
        IEnumerable<Poco> result = GetFeature(source).Metadata.References;

        // Assert
        _ = await Assert.That(result.Select(reference => reference.Qualification)).IsEquivalentTo(expected);
        _ = await Assert.That(result.All(reference => reference.HasBinder)).IsTrue();
        _ = await Assert.That(result.All(reference => !reference.IsPartial)).IsTrue();
    }

    [Test]
    [Arguments("public sealed partial record View(string Value);", true, false)]
    [Arguments("public sealed partial class View { public string Value { get; init; } }", false, false)]
    [Arguments("public partial struct View { public string Value { get; set; } }", false, true)]
    public async Task GivenViewTypesThenTheirDeclarationCharacteristicsAreCatalogued(string declaration, bool isRecord, bool isStruct)
    {
        // Arrange
        const string expectedName = "View";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(View View);
            }

            {{declaration}}
            """;

        // Act
        Poco result = GetFeature(source).Metadata.References.Single(reference => reference.Qualification.Moniker.ToString() == expectedName);

        // Assert
        _ = await Assert.That(result.Characteristics.IsRecord).IsEqualTo(isRecord);
        _ = await Assert.That(result.Characteristics.IsStruct).IsEqualTo(isStruct);
        _ = await Assert.That(result.Characteristics.IsClass).IsEqualTo(!isStruct);
    }

    [Test]
    [Arguments("public sealed partial record View(string Value);", false)]
    [Arguments("public sealed record View(string Value) : Mu.Serialization.IBinder;", true)]
    public async Task GivenViewsFromAnotherAssemblyThenOnlyExistingBindersAreCatalogued(string declaration, bool hasBinder)
    {
        // Arrange
        const string expectedName = "View";
        const string externalAssemblyName = "MooVC.Testing.Views";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
            {
                public sealed partial record Result(View View);
            }
            """;
        string externalSource = $$"""
            namespace Mu.Serialization
            {
                public interface IBinder;
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                {{declaration}}
            }
            """;
        var external = CSharpCompilation.Create(
            externalAssemblyName,
            [CSharpSyntaxTree.ParseText(externalSource)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [CSharpSyntaxTree.ParseText(SupportSource), CSharpSyntaxTree.ParseText(source)],
            _references.Append(external.ToMetadataReference()),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Act
        Poco? result = compilation.ParseFeatureModel((Area: "Mechanics", Feature: "Register", Unit: "Car"))
            .Metadata.References.SingleOrDefault(reference => reference.Qualification.Moniker.ToString() == expectedName);

        // Assert
        _ = await Assert.That(result is not null).IsEqualTo(hasBinder);

        if (result is not null)
        {
            _ = await Assert.That(result.HasBinder).IsTrue();
            _ = await Assert.That(result.IsPartial).IsFalse();
        }
    }
}