namespace Muify.Service.GenerateGrpcClientWhenFeatureVisitedTests;

using System.Collections.Generic;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed partial class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureWhenHasGrpcClientIsFalseThenClientIsGenerated()
    {
        // Arrange
        const string expectedHint = "RegisterService.Grpc.Client";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Ardalis.GuardClauses;

            partial class RegisterService
            {
                public static partial class Grpc
                {
                    public sealed partial class Client(
                        IRegisterService.IGrpc client,
                        global::MooVC.Testing.Mechanics.Car.Register.Options options,
                        global::Mu.Communications.Tracing.IScribe scribe)
                        : IRegisterService
                    {
                        public async global::System.Threading.Tasks.Task<Register.Result> Register(
                            Register register,
                            global::System.Threading.CancellationToken cancellationToken)
                        {
                            _ = global::Ardalis.GuardClauses.Guard.Against.Null(register, message: "The request must be provided.");

                            using var scope = scribe.Next(register, out global::Mu.Communications.Tracing.Ledger ledger);

                            var headers = new global::Grpc.Core.Metadata
                            {
                                { global::Mu.Communications.Tracing.CallContextExtensions.CausationHeader, ledger.Causation.ToString("D") },
                                { global::Mu.Communications.Tracing.CallContextExtensions.CorrelationHeader, ledger.Correlation.ToString("D") },
                            };

                            var callOptions = new global::Grpc.Core.CallOptions(
                                headers: headers,
                                deadline: global::System.DateTime.UtcNow.Add(options.Timeout),
                                cancellationToken: cancellationToken);

                            return await client
                                .Register(register, new global::ProtoBuf.Grpc.CallContext(callOptions))
                                .ConfigureAwait(false);
                        }
                    }
                }
            }
            """;

        var visitor = new GenerateGrpcClientWhenFeatureVisited();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasGrpcClient(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenAFeatureWhenHasGrpcClientThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateGrpcClientWhenFeatureVisited();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasGrpcClient(true));
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
        var visitor = new GenerateGrpcClientWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}