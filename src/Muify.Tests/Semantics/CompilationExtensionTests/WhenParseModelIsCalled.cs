namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MooVC.Syntax.CSharp;
using Mu.Modelling;

public sealed class WhenParseModelIsCalled
{
    private const string AssemblyName = "MooVC.Testing.Mechanics.Car";

    private const string AttributeSource = """
        namespace Muify.Domain;

        using System;

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        public sealed class IdentityAttribute
            : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class UnitAttribute<TIdentity>
            : Attribute
            where TIdentity : struct
        {
        }
        """;

    private const string CompositionSource = """
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
        """;

    private const string ServicesSource = """
        namespace Mu.Modelling.Services
        {
            public interface IAllocator<TIdentity>
                where TIdentity : struct
            {
            }
        }
        """;

    private const string StateSource = """
        namespace Mu.Modelling.State
        {
            public abstract record Aggregate;
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenAUnitAttributeThenUnitIdentityIsDiscoveredFromTheGenericArgument()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;
            """;

        Unit expected = Unit.Undefined.IdentifiedBy(typeof(Guid));

        // Act
        Model result = GetModel(source);

        // Assert
        _ = await Assert.That(result.Areas[0].Units[0].Identity).IsEqualTo(expected.Identity);
    }

    [Test]
    public async Task GivenADefaultUnitIdentityWithoutAnAllocatorThenHasAllocatorIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasAllocator;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenADefaultUnitIdentityWithAMatchingAllocatorThenHasAllocatorIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                using System;
                using Muify.Domain;

                [Unit<Guid>]
                public sealed partial record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Allocation
            {
                using System;
                using Mu.Modelling.Services;

                public sealed class GuidAllocator
                    : IAllocator<Guid>
                {
                }
            }
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasAllocator;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenAnUnnamedUnitIdentityWithAMatchingAllocatorThenHasAllocatorIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed partial record Car;
            }

            namespace MooVC.Testing.Mechanics.Car.Allocation
            {
                using System;
                using Mu.Modelling.Services;

                public sealed class GuidAllocator
                    : IAllocator<Guid>
                {
                }
            }
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasAllocator;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenADefaultUnitIdentityWithAMismatchedAllocatorThenHasAllocatorIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;

            public sealed class NumberAllocator
                : Mu.Modelling.Services.IAllocator<int>
            {
            }
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasAllocator;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenANonDefaultUnitIdentityWithAMatchingAllocatorThenHasAllocatorIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Mu.Modelling.Services;
            using Muify.Domain;

            [Unit<CarIdentity>]
            public sealed partial record Car;

            public readonly struct CarIdentity
            {
            }

            public sealed class CarIdentityAllocator
                : IAllocator<CarIdentity>
            {
            }
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasAllocator;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenAUnitWithoutAnAggregateBaseThenHasBaseIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car;
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasBase;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenAUnitWithAnAggregateBaseThenHasBaseIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Mu.Modelling.State;
            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
                : Aggregate;
            """;

        // Act
        bool result = GetModel(source).Areas[0].Units[0].Metadata.HasBase;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenClassesInTheUnitNamespaceThenRegistrarsContainsOnlyRegistrarClasses()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car
            {
                using Microsoft.Extensions.Configuration;
                using Mu.Composition;
                using Muify.Domain;
                using SimpleInjector;

                [Unit<int>]
                public sealed partial record Car;

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

            namespace MooVC.Testing.Mechanics.Car.Register
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
            (Name: "FirstRegistrar", Qualifier: "MooVC.Testing.Mechanics.Car"),
            (Name: "SecondRegistrar", Qualifier: "MooVC.Testing.Mechanics.Car"),
        ];

        // Act
        IEnumerable<Qualification> result = GetModel(source).Areas[0].Units[0].Metadata.Registrars;

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(expected);
    }

    [Test]
    public async Task GivenAnIdentityThatIsNotSelfComparableThenIdentifierComparabilityIsNotApplicable()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
            {
                [Identity]
                public Locations Location { get; set; }
            }

            public readonly struct Locations
            {
            }
            """;

        // Act
        Component.Semantics.Comparability result = GetComponent(source).Metadata.Identifier.Comparability;

        // Assert
        _ = await Assert.That(result.IsComparable).IsEqualTo(Presence.NotApplicable);
    }

    [Test]
    public async Task GivenAnIdentityThatIsNotSelfComparableWhenComparabilityMembersExistThenMembersAreIgnored()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
            {
                [Identity]
                public Locations Location { get; set; }

                public int CompareTo(Location other)
                {
                    return 0;
                }

                public static bool operator >(Wheel left, Location right)
                {
                    return true;
                }

                public static bool operator >=(Wheel left, Location right)
                {
                    return true;
                }

                public static bool operator <(Wheel left, Location right)
                {
                    return true;
                }

                public static bool operator <=(Wheel left, Location right)
                {
                    return true;
                }
            }

            public readonly struct Locations
            {
            }
            """;

        // Act
        Component.Semantics.Comparability result = GetComponent(source).Metadata.Identifier.Comparability;

        // Assert
        _ = await Assert.That(result.IsComparable).IsEqualTo(Presence.NotApplicable);
        _ = await Assert.That(result.HasCompareTo).IsFalse();
        _ = await Assert.That(result.HasGreaterThanOperator).IsFalse();
        _ = await Assert.That(result.HasGreaterThanOrEqualOperator).IsFalse();
        _ = await Assert.That(result.HasLessThanOperator).IsFalse();
        _ = await Assert.That(result.HasLessThanOrEqualOperator).IsFalse();
    }

    [Test]
    public async Task GivenAnIdentityThatIsSelfComparableWhenComponentIsNotComparableThenIdentifierComparabilityIsMissing()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
            {
                [Identity]
                public Locations Location { get; set; }
            }

            public readonly struct Locations
                : System.IComparable<Locations>
            {
                public int CompareTo(Locations other)
                {
                    return 0;
                }
            }
            """;

        // Act
        Component.Semantics.Comparability result = GetComponent(source).Metadata.Identifier.Comparability;

        // Assert
        _ = await Assert.That(result.IsComparable).IsEqualTo(Presence.Missing);
        _ = await Assert.That(result.HasCompareTo).IsFalse();
        _ = await Assert.That(result.HasGreaterThanOperator).IsFalse();
        _ = await Assert.That(result.HasGreaterThanOrEqualOperator).IsFalse();
        _ = await Assert.That(result.HasLessThanOperator).IsFalse();
        _ = await Assert.That(result.HasLessThanOrEqualOperator).IsFalse();
    }

    [Test]
    public async Task GivenAnIdentityThatIsSelfComparableWhenComponentIsComparableThenIdentifierComparabilityIsPresent()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
                : System.IComparable<Locations>
            {
                [Identity]
                public Locations Location { get; set; }

                public int CompareTo(Locations other)
                {
                    return Location.CompareTo(other);
                }
            }

            public readonly struct Locations
                : System.IComparable<Locations>
            {
                public int CompareTo(Locations other)
                {
                    return 0;
                }
            }
            """;

        // Act
        Component.Semantics.Comparability result = GetComponent(source).Metadata.Identifier.Comparability;

        // Assert
        _ = await Assert.That(result.IsComparable).IsEqualTo(Presence.Present);
    }

    [Test]
    public async Task GivenAComparableComponentWhenComparabilityMembersExistThenIdentifierComparabilityMembersArePresent()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
                : System.IComparable<Locations>
            {
                [Identity]
                public Locations Location { get; set; }

                public int CompareTo(Locations other)
                {
                    return Location.CompareTo(other);
                }

                public static bool operator >(Wheel left, Locations right)
                {
                    return true;
                }

                public static bool operator >=(Wheel left, Locations right)
                {
                    return true;
                }

                public static bool operator <(Wheel left, Locations right)
                {
                    return true;
                }

                public static bool operator <=(Wheel left, Locations right)
                {
                    return true;
                }
            }

            public readonly struct Locations
                : System.IComparable<Locations>
            {
                public int CompareTo(Locations other)
                {
                    return 0;
                }
            }
            """;

        // Act
        Component.Semantics.Comparability result = GetComponent(source).Metadata.Identifier.Comparability;

        // Assert
        _ = await Assert.That(result.IsComparable).IsEqualTo(Presence.Present);
        _ = await Assert.That(result.HasCompareTo).IsTrue();
        _ = await Assert.That(result.HasGreaterThanOperator).IsTrue();
        _ = await Assert.That(result.HasGreaterThanOrEqualOperator).IsTrue();
        _ = await Assert.That(result.HasLessThanOperator).IsTrue();
        _ = await Assert.That(result.HasLessThanOrEqualOperator).IsTrue();
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierImplicitConversionIsMissingThenIdentifierImplicitConversionIsFalse()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
            {
                [Identity]
                public Locations Location { get; set; }
            }

            public readonly struct Locations
            {
            }
            """;

        // Act
        bool result = GetComponent(source).Metadata.Identifier.HasImplicitConversion;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierImplicitConversionExistsThenIdentifierImplicitConversionIsTrue()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using Muify.Domain;

            [Unit<int>]
            public sealed partial record Car
            {
                public Wheel Wheel { get; set; }
            }

            public sealed partial class Wheel
            {
                [Identity]
                public Locations Location { get; set; }

                public static implicit operator Locations(Wheel subject)
                {
                    return subject.Location;
                }
            }

            public readonly struct Locations
            {
            }
            """;

        // Act
        bool result = GetComponent(source).Metadata.Identifier.HasImplicitConversion;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    private static Model GetModel(string source)
    {
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(AttributeSource),
                CSharpSyntaxTree.ParseText(CompositionSource),
                CSharpSyntaxTree.ParseText(ServicesSource),
                CSharpSyntaxTree.ParseText(StateSource),
                CSharpSyntaxTree.ParseText(source),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.ParseModel(CancellationToken.None);
    }

    private static Component GetComponent(string source)
    {
        return GetModel(source).Areas[0].Units[0].Components[0];
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