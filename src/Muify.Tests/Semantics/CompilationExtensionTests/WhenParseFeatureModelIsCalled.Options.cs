namespace Muify.Semantics.CompilationExtensionTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseFeatureModelIsCalled
{
    private const string OptionsSupportSource = """
        namespace Mu.Configuration
        {
            public record Options;
        }

        namespace MooVC.Testing.Mechanics.Car
        {
            public abstract record OptionsBase : Mu.Configuration.Options;
        }
        """;

    [Test]
    public async Task GivenAFeatureWithoutOptionsThenItsConfigurationIsMissing()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;
            """;

        // Act
        Feature.Semantics result = GetFeature(OptionsSupportSource + source).Metadata;

        // Assert
        _ = await Assert.That(result.Options.HasBase).IsFalse();
        _ = await Assert.That(result.Options.IsUndefined).IsTrue();
    }

    [Test]
    [Arguments("public sealed partial record RegisterOptions;", true, true, false)]
    [Arguments("public sealed record RegisterOptions;", false, true, false)]
    [Arguments("public sealed partial class RegisterOptions;", true, false, false)]
    [Arguments("public sealed partial record RegisterOptions : OptionsBase;", true, true, true)]
    [Arguments("public sealed partial record RegisterOptions : OtherBase; public abstract record OtherBase;", true, true, true)]
    public async Task GivenConventionalOptionsThenTheirDeclarationIsCatalogued(string declaration, bool isPartial, bool isRecord, bool hasBase)
    {
        // Arrange
        const string expectedName = "RegisterOptions";
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;

            {{declaration}}
            """;

        // Act
        Poco result = GetFeature(OptionsSupportSource + source).Metadata.Options;

        // Assert
        _ = await Assert.That(result.IsUndefined).IsFalse();
        _ = await Assert.That(result.IsPartial).IsEqualTo(isPartial);
        _ = await Assert.That(result.Characteristics.IsRecord).IsEqualTo(isRecord);
        _ = await Assert.That(result.HasBase).IsEqualTo(hasBase);
        _ = await Assert.That(result.Qualification.Moniker.ToString()).IsEqualTo(expectedName);
        _ = await Assert.That(result.Qualification.Qualifier.ToString()).IsEqualTo(AssemblyName);
    }

    [Test]
    [Arguments("public sealed record CustomOptions : Mu.Configuration.Options;")]
    [Arguments("public sealed record CustomOptions : OptionsBase;")]
    [Arguments("public static class Settings { public sealed record CustomOptions : OptionsBase; }")]
    [Arguments("public sealed record RegisterOptions : Mu.Configuration.Options;")]
    public async Task GivenDirectOrInheritedOptionsThenTheExistingConfigurationIsDetected(string declaration)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;

            {{declaration}}
            """;

        // Act
        bool result = GetFeature(OptionsSupportSource + source).Metadata.Options.HasBase;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenOptionsInADescendantNamespaceThenTheExistingConfigurationIsDetected()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed record Register;
            }

            namespace MooVC.Testing.Mechanics.Car.Register.Custom
            {
                public sealed record CustomOptions : OptionsBase;
            }
            """;

        // Act
        bool result = GetFeature(OptionsSupportSource + source).Metadata.Options.HasBase;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    [Arguments("MooVC.Testing.Mechanics.Car.Register", "public sealed record CustomOptions : Mu.Configuration.Options;")]
    [Arguments("MooVC.Testing.Mechanics.Car.Register", "public sealed partial record RegisterOptions;")]
    [Arguments("MooVC.Testing.Mechanics.Car.Register.Custom", "public sealed record CustomOptions : Mu.Configuration.Options;")]
    public async Task GivenOptionsFromAnotherAssemblyThenItsConfigurationRemainsMissing(string optionsNamespace, string declaration)
    {
        // Arrange
        const string optionsAssemblyName = "MooVC.Testing.Configuration";
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;
            """;
        string optionsSource = $$"""
            namespace {{optionsNamespace}};

            {{declaration}}
            """;
        var external = CSharpCompilation.Create(
            optionsAssemblyName,
            [CSharpSyntaxTree.ParseText(OptionsSupportSource), CSharpSyntaxTree.ParseText(optionsSource)],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [CSharpSyntaxTree.ParseText(SupportSource), CSharpSyntaxTree.ParseText(OptionsSupportSource), CSharpSyntaxTree.ParseText(source)],
            _references.Append(external.ToMetadataReference()),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Act
        Feature.Semantics result = compilation.ParseFeatureModel((Area: "Mechanics", Feature: "Register", Unit: "Car")).Metadata;

        // Assert
        _ = await Assert.That(result.Options.HasBase).IsFalse();
        _ = await Assert.That(result.Options.IsUndefined).IsTrue();
    }

    [Test]
    [Arguments("MooVC.Testing.Mechanics.Car")]
    [Arguments("MooVC.Testing.Mechanics.Car.Other")]
    [Arguments("MooVC.Testing.Mechanics.Car.Registered")]
    public async Task GivenOptionsOutsideTheFeatureNamespaceThenItsConfigurationRemainsMissing(string optionsNamespace)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed record Register;
            }

            namespace {{optionsNamespace}}
            {
                public sealed record CustomOptions : Mu.Configuration.Options;
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(OptionsSupportSource + source).Metadata;

        // Assert
        _ = await Assert.That(result.Options.HasBase).IsFalse();
        _ = await Assert.That(result.Options.IsUndefined).IsTrue();
    }

    [Test]
    [Arguments("Other.Options")]
    [Arguments("Mu.Configuration.Options<int>")]
    [Arguments("Mu.Configuration.Container.Options")]
    public async Task GivenASimilarlyNamedOptionsBaseThenItsConfigurationRemainsMissing(string baseType)
    {
        // Arrange
        string source = $$"""
            namespace Other
            {
                public record Options;
            }

            namespace Mu.Configuration
            {
                public record Options<TValue>;

                public static class Container
                {
                    public record Options;
                }
            }

            namespace MooVC.Testing.Mechanics.Car.Register
            {
                public sealed record Register;

                public sealed record CustomOptions : {{baseType}};
            }
            """;

        // Act
        Feature.Semantics result = GetFeature(OptionsSupportSource + source).Metadata;

        // Assert
        _ = await Assert.That(result.Options.HasBase).IsFalse();
        _ = await Assert.That(result.Options.IsUndefined).IsTrue();
    }

    [Test]
    public async Task GivenGenericConventionalOptionsThenTheNonGenericConfigurationRemainsMissing()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;

            public sealed partial record RegisterOptions<TValue>;
            """;

        // Act
        Feature.Semantics result = GetFeature(OptionsSupportSource + source).Metadata;

        // Assert
        _ = await Assert.That(result.Options.HasBase).IsFalse();
        _ = await Assert.That(result.Options.IsUndefined).IsTrue();
    }
}