namespace Muify.Domain.UnitBaseVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAPartialUnitWhenHasBaseIsFalseThenBaseDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            public sealed partial record Car
                : global::Mu.Modelling.State.Aggregate;
            """;

        var visitor = new UnitBaseVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.IsPartial(true).HasBase(false));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(car.Name);
    }

    [Test]
    public async Task GivenAPartialUnitWhenHasBaseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBaseVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasBase(true));

        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenHasBaseIsFalseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBaseVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata
            .IsPartial(false)
            .HasBase(false));

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