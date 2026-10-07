namespace Muify.Service.GenerateGrpcServiceWhenFeatureVisitedTests;

using System.Collections.Generic;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenHasGrpcServiceIsFalseThenServiceIsGenerated(bool hasServiceContract)
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Ardalis.GuardClauses;

            partial class RegisterService
            {
                public static partial class Grpc
                {
                    public sealed partial class Service(
                        global::Mu.Communications.Mediation.IMediator mediator,
                        global::Mu.Auditing.IScopeManager manager,
                        global::Mu.Communications.Tracing.IScribe scribe)
                        : IRegisterService.IGrpc
                    {
                        public async global::System.Threading.Tasks.ValueTask<global::Mu.Result<global::MooVC.Testing.Mechanics.Car.Registration>> Register(
                            global::MooVC.Testing.Mechanics.Car.Register.Register register,
                            global::ProtoBuf.Grpc.CallContext context = default)
                        {
                            _ = global::Ardalis.GuardClauses.Guard.Against.Null(register, message: "The request must be provided.");

                            global::Mu.Communications.Tracing.Ledger ledger = global::Mu.Communications.Tracing.CallContextExtensions.ToLedger(context, register);

                            using (manager.Begin(global::Mu.Auditing.Scope.External))
                            {
                                using (scribe.Set(ledger))
                                {
                                    return await mediator
                                        .Execute<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>(
                                            register,
                                            context.CancellationToken)
                                        .ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
            """;

        var visitor = new GenerateGrpcServiceWhenFeatureVisited();

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasGrpcService(false)
            .HasGrpcServiceContract(hasServiceContract));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenAFeatureWhenHasGrpcServiceThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateGrpcServiceWhenFeatureVisited();

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasGrpcService(true)
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
        var visitor = new GenerateGrpcServiceWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}