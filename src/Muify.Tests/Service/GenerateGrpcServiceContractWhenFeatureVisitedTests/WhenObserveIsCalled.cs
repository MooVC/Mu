namespace Muify.Service.GenerateGrpcServiceContractWhenFeatureVisitedTests;

using System.Collections.Generic;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenHasGrpcServiceContractIsFalseThenServiceContractIsGenerated(bool hasService)
    {
        // Arrange
        const string expectedHint = "IRegisterService.Grpc";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            partial interface IRegisterService
            {
                [global::ProtoBuf.Grpc.Configuration.Service("MooVC.Testing.Mechanics.Car.Register.Service")]
                public partial interface IGrpc
                {
                    [global::ProtoBuf.Grpc.Configuration.Operation("Register")]
                    global::System.Threading.Tasks.ValueTask<Register.Result> Register(Register register, global::ProtoBuf.Grpc.CallContext context = default);
                }
            }
            """;

        var visitor = new GenerateGrpcServiceContractWhenFeatureVisited();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasGrpcService(hasService)
            .HasGrpcServiceContract(false));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenAFeatureWhenHasGrpcServiceContractThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateGrpcServiceContractWhenFeatureVisited();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasGrpcServiceContract(true)
            .IsPartial(true));

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
        var visitor = new GenerateGrpcServiceContractWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}