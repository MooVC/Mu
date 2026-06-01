namespace Muify.Domain.ComponentSelfComparabilityHasLessThanOperatorVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenSelfHasLessThanOperatorIsFalseThenLessThanSelfDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
            {
                public static bool operator <(Wheel left, Wheel right)
                {
                    return left is not null && left.CompareTo(right) < 0;
                }
            }
            """;

        var visitor = new ComponentSelfComparabilityHasLessThanOperatorVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability
                        .IsComparable(Presence.Present)))
                .WithSelf(self => self
                    .WithComparability(comparability => comparability
                        .HasLessThanOperator(false)
                        .IsComparable(Presence.Present))));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Self.Comparison.LessThan");
    }

    [Test]
    public async Task GivenAComponentWhenSelfHasLessThanOperatorThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentSelfComparabilityHasLessThanOperatorVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithComparability(comparability => comparability
                        .IsComparable(Presence.Present)))
                .WithSelf(self => self
                    .WithComparability(comparability => comparability
                        .HasLessThanOperator(true)
                        .IsComparable(Presence.Present))));

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
        var visitor = new ComponentSelfComparabilityHasLessThanOperatorVisitor();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}