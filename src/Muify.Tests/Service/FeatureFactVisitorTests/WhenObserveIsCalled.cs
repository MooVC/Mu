namespace Muify.Service.FeatureFactVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureWhenHasFactIsFalseThenFactDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using System.Text.Json.Serialization;
            using MooVC.Testing.Mechanics.Car;
            using Mu.Modelling.Behavior;

            public sealed partial record Registered
                : global::Mu.Modelling.Behavior.Fact<global::MooVC.Testing.Mechanics.Car.Car>,
                  global::Mu.Modelling.Behavior.IConvertFrom<global::MooVC.Testing.Mechanics.Car.Register.Registered, global::MooVC.Testing.Mechanics.Car.Register.Register>
            {
                internal Registered(byte doors, string make, string model)
                {
                    Doors = doors;
                    Make = make;
                    Model = model;
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                internal Registered(byte doors, Guid identity, string make, string model, DateTimeOffset proposed)
                    : base(identity, proposed)
                {
                    Doors = doors;
                    Make = make;
                    Model = model;
                }
            
                public byte Doors { get; init; }

                public string Make { get; init; }

                public string Model { get; init; }

                public static implicit operator Registered(Register subject)
                {
                    return new Registered(subject.Doors, subject.Make, subject.Model);
                }
            }
            """;

        var visitor = new FeatureFactVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasFact(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Mutational.Fact);
    }

    [Test]
    public async Task GivenAFeatureWithoutPayloadWhenHasFactIsFalseThenFactDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using System.Text.Json.Serialization;
            using MooVC.Testing.Mechanics.Car;
            using Mu.Modelling.Behavior;

            public sealed partial record Registered
                : global::Mu.Modelling.Behavior.Fact<global::MooVC.Testing.Mechanics.Car.Car>,
                  global::Mu.Modelling.Behavior.IConvertFrom<global::MooVC.Testing.Mechanics.Car.Register.Registered, global::MooVC.Testing.Mechanics.Car.Register.Register>
            {
                internal Registered()
                {
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                internal Registered(Guid identity, DateTimeOffset proposed)
                    : base(identity, proposed)
                {
                }

                public static implicit operator Registered(Register subject)
                {
                    return new Registered();
                }
            }
            """;

        var visitor = new FeatureFactVisitor();

        Feature register = Feature.Undefined
            .Named(TestData.Single.Register.Value.Name)
            .IsMutational(_ => TestData.Single.Register.Value.Mutational)
            .WithMetadata(metadata => metadata.HasFact(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Mutational.Fact);
    }

    [Test]
    public async Task GivenAFeatureWhenHasFactThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureFactVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasFact(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenACFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureFactVisitor();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}