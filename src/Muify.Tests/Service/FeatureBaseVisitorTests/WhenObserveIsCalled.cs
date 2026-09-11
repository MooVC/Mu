namespace Muify.Service.FeatureBaseVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAPartialCreationalFeatureWhenHasBaseIsFalseThenBaseDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial record Register
                : global::Mu.Modelling.Behavior.Creational<global::MooVC.Testing.Mechanics.Car.Car>;
            """;

        var visitor = new FeatureBaseVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.IsPartial(true).HasBase(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Name);
    }

    [Test]
    public async Task GivenAPartialTransitionalFeatureWhenHasBaseIsFalseThenBaseDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Unregister;

            public sealed partial record Unregister
                : global::Mu.Modelling.Behavior.Transitional<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Registration>;
            """;

        var visitor = new FeatureBaseVisitor();
        Feature unregister = TestData.Single.Unregister.Value.WithMetadata(metadata => metadata.IsPartial(true).HasBase(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 2, TestData.Single.Model, unregister);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(unregister.Name);
    }

    [Test]
    public async Task GivenAPartialQueryFeatureWhenHasBaseIsFalseThenBaseDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.FindCarsBy;

            public sealed partial record FindCarsBy
                : global::Mu.Modelling.Behavior.Query<global::MooVC.Testing.Mechanics.Car.Car>;
            """;

        var visitor = new FeatureBaseVisitor();
        Feature findCarsBy = TestData.Single.FindCarsBy.Value.WithMetadata(metadata => metadata.IsPartial(true).HasBase(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 0, TestData.Single.Model, findCarsBy);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(findCarsBy.Name);
    }

    [Test]
    public async Task GivenAPartialFeatureWhenHasBaseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBaseVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasBase(true));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenHasBaseIsFalseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBaseVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .IsPartial(false)
            .HasBase(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureBaseVisitor();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}