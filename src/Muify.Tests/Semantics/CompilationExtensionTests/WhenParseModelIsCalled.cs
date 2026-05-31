namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

    private static Model GetModel(string source)
    {
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(AttributeSource),
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