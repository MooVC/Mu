namespace Muify.Domain.GenerateGetHashCodeOverrideWhenComponentVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenHasGetHashCodeOverrideIsFalseThenEquatableToIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
            {
                public override int GetHashCode()
                {
                    return global::System.HashCode.Combine(Location);
                }
            }
            """;

        var visitor = new GenerateGetHashCodeOverrideWhenComponentVisited();
        Component wheel = TestData.Single.Units._Value[0].Components[1].WithMetadata(metadata => metadata.HasGetHashCodeOverride(false));
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{component.Namespace}.{wheel.Name}.GetHashCode");
    }

    [Test]
    public async Task GivenAComponentWhenHasGetHashCodeOverrideThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateGetHashCodeOverrideWhenComponentVisited();
        Component wheel = TestData.Single.Units._Value[0].Components[1].WithMetadata(metadata => metadata.HasGetHashCodeOverride(true));
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
        var visitor = new GenerateGetHashCodeOverrideWhenComponentVisited();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}