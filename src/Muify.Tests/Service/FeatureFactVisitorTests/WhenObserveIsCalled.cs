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

            public sealed partial record Registered(byte Doors, string Make, string Model)
                : global::Mu.Modelling.Behavior.Fact<global::MooVC.Testing.Mechanics.Car.Car>,
                  global::Mu.Modelling.Behavior.IConvertFrom<global::MooVC.Testing.Mechanics.Car.Register.Registered, global::MooVC.Testing.Mechanics.Car.Register.Register>
            {
                [global::System.Text.Json.Serialization.JsonConstructorAttribute]
                public Registered(byte doors, Guid identity, string make, string model, DateTimeOffset proposed)
                    : base(identity, proposed)
                {
                    Doors = doors;
                    Make = make;
                    Model = model;
                }
            
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