namespace Muify.Domain.ComponentIdentifierComparabilityHasGreaterThanOrEqualOperatorVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenIdentifierHasGreaterThanOrEqualOperatorIsFalseThenGreaterThanOrEqualIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
            {
                public static bool operator >=(Wheel left, global::MooVC.Testing.Mechanics.Car.Locations right)
                {
                    return left is not null && left.CompareTo(right) >= 0;
                }
            }
            """;

        var visitor = new ComponentIdentifierComparabilityHasGreaterThanOrEqualOperatorVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability
                        .HasGreaterThanOrEqualOperator(false)
                        .IsComparable(Presence.Missing))));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Identifier.Comparison.GreaterThanOrEqual");
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierHasGreaterThanOrEqualOperatorThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentIdentifierComparabilityHasGreaterThanOrEqualOperatorVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability.HasGreaterThanOrEqualOperator(true))));

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
        var visitor = new ComponentIdentifierComparabilityHasGreaterThanOrEqualOperatorVisitor();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}