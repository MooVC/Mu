namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed class WhenParseFeatureModelIsCalled
{
    private const string AssemblyName = "MooVC.Testing.Mechanics.Car.Register";

    private const string SupportSource = """
        namespace Mu.Modelling.Behavior
        {
            public abstract record Fact;

            public abstract record UseCase;

            public abstract record Creational<TAggregate>
                : UseCase;
        }

        namespace Mu.Modelling.Services
        {
            public interface ITransform<TAggregate, TFact>
            {
            }
        }

        namespace Muify.Service
        {
            using System;

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class CreationalAttribute<TFact>
                : Attribute
            {
            }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class TransitionalAttribute<TFact>
                : Attribute
            {
            }
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenARequestWithoutAUseCaseBaseThenHasBaseIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasBase;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenARequestWithAUseCaseBaseThenHasBaseIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Mu.Modelling.Behavior;

            public sealed record Car;

            public sealed record Register
                : Creational<Car>;
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasBase;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenACreationalAttributeReferencingAMissingClassThenHasFactIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Muify.Modelling.Services;

            [Creational<MissingFact>]
            public sealed record Register;
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasFact;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenATransitionalAttributeReferencingAnExistingClassThenHasFactIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Mu.Modelling.Behavior;
            using Muify.Service;

            [Transitional<Registered>]
            public sealed record Register;

            public sealed record Registered
                : Fact;
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasFact;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenClassesInTheRequestNamespaceThenTransformsContainsOnlyTransformClasses()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register
            {
                using Mu.Modelling.Services;

                public sealed record Car;

                public sealed record Register;

                public sealed record Registered;

                public sealed class FirstTransform
                    : ITransform<Car, Registered>
                {
                }

                public sealed class NotATransform
                {
                }

                public sealed class SecondTransform
                    : ITransform<Car, Registered>
                {
                }
            }

            namespace MooVC.Testing.Mechanics.Car.Other
            {
                using Mu.Modelling.Services;

                public sealed class OtherTransform
                    : ITransform<object, object>
                {
                }
            }
            """;

        Qualification[] expected =
        [
            (Name: "FirstTransform", Qualifier: "MooVC.Testing.Mechanics.Car.Register"),
            (Name: "SecondTransform", Qualifier: "MooVC.Testing.Mechanics.Car.Register"),
        ];

        // Act
        IEnumerable<Qualification> result = GetFeature(source).Metadata.Transforms;

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }

    private static Feature GetFeature(string source)
    {
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(SupportSource),
                CSharpSyntaxTree.ParseText(source),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.ParseFeatureModel((Area: "Mechanics", Feature: "Register", Unit: "Car"));
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