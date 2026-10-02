namespace Muify.Semantics.CompilationExtensionTests;

using Mu.Modelling;

public sealed partial class WhenParseModelIsCalled
{
    [Test]
    public async Task GivenAPartialEntityThenMissingCapabilitiesAreEnabled()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car(Wheel Wheel);

            public sealed partial class Wheel
            {
                [Muify.Domain.Identity]
                public int Location { get; set; }
            }
            """;

        // Act
        Component.Semantics result = GetComponent(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasBinder).IsFalse();
        _ = await Assert.That(result.HasEqualsOverride).IsFalse();
        _ = await Assert.That(result.HasGetHashCodeOverride).IsFalse();
        _ = await Assert.That(result.Identifier.Equality.HasEquatable).IsFalse();
        _ = await Assert.That(result.Identifier.Equality.IsEquatable).IsFalse();
        _ = await Assert.That(result.Identifier.Equality.HasEqualsOperator).IsFalse();
        _ = await Assert.That(result.Identifier.Equality.HasNotEqualsOperator).IsFalse();
        _ = await Assert.That(result.Self.Equality.HasEquatable).IsFalse();
        _ = await Assert.That(result.Self.Equality.IsEquatable).IsFalse();
        _ = await Assert.That(result.Self.Equality.HasEqualsOperator).IsFalse();
        _ = await Assert.That(result.Self.Equality.HasNotEqualsOperator).IsFalse();
        _ = await Assert.That(result.Self.Comparability.IsComparable).IsEqualTo(Presence.Missing);
    }

    [Test]
    public async Task GivenExistingEqualityMembersThenTheirVisitorsAreDisabled()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car(Wheel Wheel);

            public sealed partial class Wheel : System.IEquatable<int>, System.IEquatable<Wheel>
            {
                [Muify.Domain.Identity]
                public int Location { get; set; }

                public bool Equals(int other) => Location == other;

                public bool Equals(Wheel other) => other is not null && Equals(other.Location);

                public override bool Equals(object other) => other is Wheel wheel && Equals(wheel);

                public override int GetHashCode() => Location;

                public static bool operator ==(Wheel left, int right) => left.Equals(right);

                public static bool operator !=(Wheel left, int right) => !left.Equals(right);

                public static bool operator ==(Wheel left, Wheel right) => left.Equals(right);

                public static bool operator !=(Wheel left, Wheel right) => !left.Equals(right);
            }
            """;

        // Act
        Component.Semantics result = GetComponent(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasEqualsOverride).IsTrue();
        _ = await Assert.That(result.HasGetHashCodeOverride).IsTrue();
        _ = await Assert.That(result.Identifier.Equality.IsOutOfScope).IsTrue();
        _ = await Assert.That(result.Self.Equality.IsOutOfScope).IsTrue();
    }

    [Test]
    [Arguments("class")]
    [Arguments("record")]
    public async Task GivenAPartialComponentWithoutAnIdentityThenItsBinderIsEnabled(string kind)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car(Wheel Wheel);

            public sealed partial {{kind}} Wheel
            {
                public int Size { get; set; }
            }
            """;

        // Act
        Component.Semantics result = GetComponent(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasBinder).IsFalse();
    }

    [Test]
    [Arguments("class")]
    [Arguments("record")]
    public async Task GivenANonPartialComponentThenItsVisitorsRemainOutOfScope(string kind)
    {
        // Arrange
        string source = $$"""
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car(Wheel Wheel);

            public sealed {{kind}} Wheel
            {
                [Muify.Domain.Identity]
                public int Location { get; set; }
            }
            """;

        // Act
        Component.Semantics result = GetComponent(source).Metadata;

        // Assert
        _ = await Assert.That(result.IsOutOfScope).IsTrue();
    }

    [Test]
    public async Task GivenAPartialStructThenOnlyItsStructCompatibleBinderIsEnabled()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            public sealed record Car(Wheel Wheel);

            public partial struct Wheel
            {
                [Muify.Domain.Identity]
                public int Location { get; set; }
            }
            """;

        // Act
        Component.Semantics result = GetComponent(source).Metadata;

        // Assert
        _ = await Assert.That(result.HasBinder).IsFalse();
        _ = await Assert.That(result.Identifier.IsOutOfScope).IsTrue();
        _ = await Assert.That(result.Self.IsOutOfScope).IsTrue();
    }
}