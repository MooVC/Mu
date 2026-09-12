namespace Muify.Service.FeatureConstructorsVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAPartialCreationalFeatureThenJsonConstructorRestoresThePayloadAndCause()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using System.Text.Json.Serialization;

            public sealed partial record Register
            {
                public Register()
                {
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                internal Register(byte doors, Guid identity, string make, string model, DateTimeOffset proposed)
                    : base(identity, proposed)
                {
                    Doors = doors;
                    Make = make;
                    Model = model;
                }
            }
            """;

        var visitor = new FeatureConstructorsVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.IsPartial(true).HasConstructors(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint.ToString()).IsEqualTo("Register.ctor");
    }

    [Test]
    public async Task GivenAPartialTransitionalFeatureThenJsonConstructorRestoresTheTargetAndCause()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Unregister;

            using System;
            using System.Text.Json.Serialization;
            using MooVC.Testing.Mechanics.Car;
            using Mu.Modelling.State;

            public sealed partial record Unregister
            {
                public Unregister()
                    : base(target: default)
                {
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                internal Unregister(Guid identity, DateTimeOffset proposed, Reference<Registration> target)
                    : base(identity, proposed, target)
                {
                }
            }
            """;

        var visitor = new FeatureConstructorsVisitor();
        Feature unregister = TestData.Single.Unregister.Value.WithMetadata(metadata => metadata.IsPartial(true).HasConstructors(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 2, TestData.Single.Model, unregister);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint.ToString()).IsEqualTo("Unregister.ctor");
    }

    [Test]
    public async Task GivenAPartialQueryFeatureThenJsonConstructorRestoresThePayloadAndCause()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.FindCarsBy;

            using System;
            using System.Text.Json.Serialization;

            public sealed partial record FindCarsBy
            {
                public FindCarsBy()
                {
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                internal FindCarsBy(Guid identity, string? make, string? model, DateTimeOffset proposed)
                    : base(identity, proposed)
                {
                    Make = make;
                    Model = model;
                }
            }
            """;

        var visitor = new FeatureConstructorsVisitor();
        Feature findCarsBy = TestData.Single.FindCarsBy.Value.WithMetadata(metadata => metadata.IsPartial(true).HasConstructors(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 0, TestData.Single.Model, findCarsBy);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint.ToString()).IsEqualTo("FindCarsBy.ctor");
    }

    [Test]
    public async Task GivenAPartialFeatureWhenHasConstructorsThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureConstructorsVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.IsPartial(true).HasConstructors(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenANonPartialFeatureWhenHasConstructorsIsFalseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureConstructorsVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.IsPartial(false).HasConstructors(false));
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
        var visitor = new FeatureConstructorsVisitor();

        // Act
        IEnumerable<File> result = visitor.Observe(TestData.Single.Register);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}