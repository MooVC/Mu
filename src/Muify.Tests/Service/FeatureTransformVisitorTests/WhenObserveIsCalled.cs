namespace Muify.Service.FeatureTransformVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAMutationalFeatureWhenQualificationsAreEmptyThenTransformDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using MooVC.Testing.Mechanics.Car;

            internal sealed class Transform
                : global::Mu.Modelling.Services.ITransform<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Register.Registered>
            {
                public Car Apply(global::MooVC.Testing.Mechanics.Car.Car aggregate, global::MooVC.Testing.Mechanics.Car.Register.Registered fact)
                {
                    return aggregate with
                    {
                        Doors = fact.Doors,
                        Make = fact.Make,
                        Model = fact.Model,
                    };
                }
            }
            """;

        var visitor = new FeatureTransformVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasFact(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Transform");
    }

    [Test]
    public async Task GivenAMutationalFeatureWhenQualificationsExistThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureTransformVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasFact(false)
            .WithTransforms("MooVC.Testing.Transform"));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenANonMutationalFeatureWhenQualificationsAreEmptyThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureTransformVisitor();
        Feature findCarsBy = TestData.Single.FindCarsBy.Value.WithMetadata(metadata => metadata.HasFact(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 0, TestData.Single.Model, findCarsBy);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureTransformVisitor();

        // Act
        IEnumerable<File> result = visitor.Observe(TestData.Single.Register);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}