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

            public sealed partial record Registered
                : global::Mu.Modelling.Behavior.Fact<global::MooVC.Testing.Mechanics.Car.Car>
            {
                public Registered(byte doors, string make, string model)
                {
                    Doors = doors;
                    Make = make;
                    Model = model;
                }

                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                public Registered(Guid identity, DateTimeOffset proposed, byte doors, string make, string model)
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
}