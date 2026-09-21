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
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            partial class RegisterService
            {
                public sealed partial class Grpc(
                    global::Mu.Communications.Mediation.IHandler<Register,
                    Register.Result> handler,
                    global::Mu.Auditing.IScopeManager manager,
                    global::Mu.Communications.Tracing.IScribe scribe)
                    : IRegisterService.IGrpc
                {
                    public async global::System.Threading.Tasks.ValueTask<Register.Result> Register(
                        Register register,
                        global::ProtoBuf.Grpc.CallContext context = default)
                    {
                        global::Mu.Communications.Tracing.Ledger ledger = global::Mu.Communications.Tracing.CallContextExtensions.ToLedger(context, register);

                        using (manager.Begin(global::Mu.Auditing.Scope.External))
                        {
                            using (scribe.Set(ledger))
                            {
                                return await handler
                                    .Handle(register, context.CancellationToken)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            """;

        var visitor = new GenerateGrpcServiceWhenFeatureVisited();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasGrpcService(false)
            .HasGrpcServiceContract(hasServiceContract));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Name);
    }

    [Test]
    public async Task GivenAFeatureWhenHasGrpcServiceThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateGrpcServiceWhenFeatureVisited();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
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