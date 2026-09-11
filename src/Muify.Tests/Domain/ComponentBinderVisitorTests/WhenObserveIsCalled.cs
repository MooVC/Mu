namespace Muify.Domain.ComponentBinderVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAnEntityWhenHasBinderIsFalseThenBinderDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial class Wheel
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(Wheel), false);

                    meta.UseConstructor = false;

                    meta.Add(1, "Location");
                    meta.Add(2, "Pressure");

                    return model;
                }
            }
            """;

        var visitor = new ComponentBinderVisitor();

        Component wheel = TestData.Single.Units.Value[0].Components[1].WithMetadata(metadata => metadata
            .WithCharacteristics(characteristics => characteristics.IsClass(true))
            .HasBinder(false));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Wheel.Binder");
    }

    [Test]
    public async Task GivenAValueWhenHasBinderIsFalseThenBinderDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            partial record Pressure
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(Pressure), false);

                    meta.UseConstructor = false;

                    meta.Add(1, "Unit");
                    meta.Add(2, "Value");

                    return model;
                }
            }
            """;

        var visitor = new ComponentBinderVisitor();

        Component pressure = TestData.Single.Units.Value[0].Components[0].WithMetadata(metadata => metadata
            .WithCharacteristics(characteristics => characteristics.IsRecord(true))
            .HasBinder(false));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, pressure);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Pressure.Binder");
    }

    [Test]
    public async Task GivenAUnitWhenHasBinderThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentBinderVisitor();
        Component wheel = TestData.Single.Units.Value[0].Components[1].WithMetadata(metadata => metadata.HasBinder(true));
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new ComponentBinderVisitor();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}