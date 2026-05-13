namespace Muify.Domain.UnitBaseInspectorTests;

extern alias Modelling;

using Modelling::Mu.Modelling;
using Mu.Modelling;
using Muify;
using Model = Modelling::Mu.Modelling.Model;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitWhenHasBaseIsFalseThenBaseDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car
            {
                public sealed partial record Car
                    : global::Mu.Modelling.State.Aggregate;
            }
            """;

        var visitor = new UnitBaseVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasBase(false));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(car.Name);
    }

    [Test]
    public async Task GivenAUnitWhenHasBaseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBaseVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasBase(true));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBaseVisitor();
        Model.Graph.Areas.Area.Units.Unit unit = TestData.Single.Car;

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}