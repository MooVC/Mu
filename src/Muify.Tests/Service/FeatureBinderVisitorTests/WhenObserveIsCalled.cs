namespace Muify.Service.FeatureBinderVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAPartialRequestWhenHasBinderIsFalseThenBinderDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            partial record Register
                : global::Mu.Serialization.IBinder
            {
                public static global::ProtoBuf.Meta.RuntimeTypeModel Bind(global::ProtoBuf.Meta.RuntimeTypeModel model)
                {
                    var meta = model.Add(typeof(Register), false);

                    meta.UseConstructor = false;

                    meta.Add(1, "Identity");
                    meta.Add(2, "Proposed");
                    meta.Add(3, "Model");
                    meta.Add(4, "Doors");
                    meta.Add(5, "Make");
                    meta.Add(6, "Model");

                    return model;
                }
            }
            """;

        var visitor = new FeatureBinderVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasBinder(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Binder");
    }

    [Test]
    public async Task GivenAPartialRequestWhenHasBinderThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBinderVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasBinder(true));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenARequestWhenHasBinderIsFalseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBinderVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .IsPartial(false)
            .HasBinder(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBinderVisitor();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}