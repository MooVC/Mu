namespace Muify.Domain.GenerateIdentifierEqualityEqualsWhenComponentVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenIdentifierHasEquatableIsFalseThenEquatableToIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
            {
                public bool Equals(global::MooVC.Testing.Mechanics.Car.Locations other)
                {
                    return global::System.Collections.Generic.EqualityComparer<global::MooVC.Testing.Mechanics.Car.Locations>.Default.Equals(Location, other);
                }
            }
            """;

        var visitor = new GenerateIdentifierEqualityEqualsWhenComponentVisited();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithEquality(equality => equality.HasEquatable(false))));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Identifier.IEquatable.Equals");
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierHasEquatableThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateIdentifierEqualityEqualsWhenComponentVisited();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .WithEquality(equality => equality.HasEquatable(true))));

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
        var visitor = new GenerateIdentifierEqualityEqualsWhenComponentVisited();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}