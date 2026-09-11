namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;
using Identifier = MooVC.Syntax.Identifier;

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

        namespace Microsoft.Extensions.Configuration
        {
            public interface IConfiguration
            {
            }
        }

        namespace SimpleInjector
        {
            public sealed class Container
            {
            }
        }

        namespace Mu.Composition
        {
            using Microsoft.Extensions.Configuration;
            using SimpleInjector;

            public interface IRegistrar
            {
                static abstract Container Register(IConfiguration configuration, Container container);
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
    public async Task GivenACreationalAttributeReferencingAMissingClassThenFactIsIdentifiedForGeneration()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Muify.Service;

            [Creational<MissingFact>]
            public sealed record Register;
            """;

        // Act
        Feature result = GetFeature(source);

        // Assert
        _ = await Assert.That(result.Metadata.HasFact).IsFalse();
        _ = await Assert.That(result.Mutational.Fact.ToString()).IsEqualTo("MissingFact");
        _ = await Assert.That(result.Mutational.Type.IsCreational).IsTrue();
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
        Feature result = GetFeature(source);

        // Assert
        _ = await Assert.That(result.Metadata.HasFact).IsTrue();
        _ = await Assert.That(result.Mutational.Fact.ToString()).IsEqualTo("Registered");
        _ = await Assert.That(result.Mutational.Type.IsTransitional).IsTrue();
    }

    [Test]
    public async Task GivenPublicInstancePropertiesThenParametersRetainTheirTypesInAlphabeticalOrder()
    {
        // Arrange
        const string source = """
            #nullable enable

            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register(string[] Owners, string? Description)
            {
                public static int Count { get; set; }

                private int State { get; set; }

                public sealed record Result(string Value);
            }
            """;

        string[] expected = ["Description", "Owners"];

        // Act
        Feature result = GetFeature(source);

        // Assert
        _ = await Assert.That(result.Parameters.Select(parameter => parameter.Name.ToSnippet(Identifier.Options.Pascal).ToString())).IsEquivalentTo(expected);
        _ = await Assert.That(result.Parameters[0].Type.IsNullable).IsTrue();
        _ = await Assert.That(result.Parameters[1].Type.IsArray).IsTrue();
    }

    [Test]
    public async Task GivenARequestWithoutARegistrarThenHasRegistrarIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed record Register;
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasRegistrar;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenARequestWithARegistrarThenHasRegistrarIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Microsoft.Extensions.Configuration;
            using Mu.Composition;
            using SimpleInjector;

            public sealed record Register
                : IRegistrar
            {
                public static Container Register(IConfiguration configuration, Container container)
                {
                    return container;
                }
            }
            """;

        // Act
        bool result = GetFeature(source).Metadata.HasRegistrar;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenClassesInTheRequestNamespaceThenRegistrarsContainsOnlyRegistrarClasses()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car.Register
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using SimpleInjector;

                public sealed record Register;

                public sealed class FirstRegistrar
                    : IRegistrar
                {
                    public static Container Register(IConfiguration configuration, Container container)
                    {
                        return container;
                    }
                }

                public sealed class NotARegistrar
                {
                }

                public sealed class SecondRegistrar
                    : IRegistrar
                {
                    public static Container Register(IConfiguration configuration, Container container)
                    {
                        return container;
                    }
                }
            }

            namespace MooVC.Testing.Mechanics.Car.Other
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using SimpleInjector;

                public sealed class OtherRegistrar
                    : IRegistrar
                {
                    public static Container Register(IConfiguration configuration, Container container)
                    {
                        return container;
                    }
                }
            }
            """;

        Qualification[] expected =
        [
            (Name: "FirstRegistrar", Qualifier: "MooVC.Testing.Mechanics.Car.Register"),
            (Name: "SecondRegistrar", Qualifier: "MooVC.Testing.Mechanics.Car.Register"),
        ];

        // Act
        IEnumerable<Qualification> result = GetFeature(source).Metadata.Registrars;

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
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