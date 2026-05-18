namespace Muify.Domain.ComponentIdentifierHasEqualsOperatorVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenIdentifierHasEqualsOperatorIsFalseThenEquatableToIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;
            
            partial class Wheel
            {
                public static bool operator ==(Wheel left, global::MooVC.Testing.Mechanics.Car.Locations right)
                {
                    return left is not null && left.Equals(right);
                }
            }
            """;

        var visitor = new ComponentIdentifierHasEqualsOperatorVisitor();
        Component wheel = TestData.Single.Units.Value[0].Components[1].WithMetadata(metadata => metadata.WithIdentifier(identifier => identifier.HasEqualsOperator(false)));
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Identifier.Comparison.Equals");
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierHasEqualsOperatorThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentIdentifierHasEqualsOperatorVisitor();
        Component wheel = TestData.Single.Units.Value[0].Components[1].WithMetadata(metadata => metadata.WithIdentifier(identifier => identifier.HasEqualsOperator(true)));
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
        var visitor = new ComponentIdentifierHasEqualsOperatorVisitor();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}