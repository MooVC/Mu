namespace Muify.Domain.UnitBinderVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitWhenHasBinderIsFalseThenBinderDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial record Car
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(Car), false);

                    meta.UseConstructor = false;

                    meta.Add(1, "Propositions");
                    meta.Add(2, "Revision");
                    meta.Add(3, "Doors");
                    meta.Add(4, "Make");
                    meta.Add(5, "Model");
                    meta.Add(6, "Wheels");

                    return model;
                }
            }
            """;

        var visitor = new UnitBinderVisitor();
        Model model = TestData.Single.Model;
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasBinder(false));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Binder");
    }

    [Test]
    public async Task GivenAUnitWhenHasBinderThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBinderVisitor();
        Model model = TestData.Single.Model;
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasBinder(true));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitBinderVisitor();
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, TestData.Single.Car.Value);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}