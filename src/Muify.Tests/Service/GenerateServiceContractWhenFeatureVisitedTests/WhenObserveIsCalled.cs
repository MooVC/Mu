namespace Muify.Service.GenerateServiceContractWhenFeatureVisitedTests;

using System.Collections.Generic;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenHasServiceContractIsFalseThenServiceContractIsGenerated(bool hasService)
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public partial interface IRegisterService
            {
                global::System.Threading.Tasks.Task<Register.Result> Register(Register register, global::System.Threading.CancellationToken cancellationToken);
            }
            """;

        var visitor = new GenerateServiceContractWhenFeatureVisited();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasService(hasService)
            .HasServiceContract(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Name);
    }

    [Test]
    public async Task GivenAFeatureWhenHasServiceContractThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateServiceContractWhenFeatureVisited();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasServiceContract(true));
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
        var visitor = new GenerateServiceContractWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}