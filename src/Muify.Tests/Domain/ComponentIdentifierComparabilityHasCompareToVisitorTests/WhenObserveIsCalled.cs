namespace Muify.Domain.ComponentIdentifierComparabilityHasCompareToVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenIdentifierHasCompareToIsFalseThenCompareToIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
            {
                public int CompareTo(global::MooVC.Testing.Mechanics.Car.Locations? other)
                {
                    return other is null ? 1 : other.CompareTo(Location);
                }
            }
            """;

        var visitor = new ComponentIdentifierComparabilityHasCompareToVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability
                        .HasCompareTo(false)
                        .IsComparable(Presence.Missing))));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Identifier.IComparable.CompareTo");
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierHasCompareToThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentIdentifierComparabilityHasCompareToVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability.HasCompareTo(true))));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAComponentWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentIdentifierComparabilityHasCompareToVisitor();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}