namespace Muify.Service.GenerateServiceWhenFeatureVisitedTests;

using System.Collections.Generic;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAFeatureWhenHasServiceIsFalseThenServiceIsGenerated(bool hasServiceContract)
    {
        // Arrange
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Register.RegisterService";
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using Ardalis.GuardClauses;

            public sealed partial class RegisterService(
                global::Mu.Communications.Mediation.IMediator mediator,
                global::Mu.Auditing.IScopeManager manager,
                global::Mu.Communications.Tracing.IScribe scribe)
                : IRegisterService
            {
                public async global::System.Threading.Tasks.Task<global::Mu.Result<global::MooVC.Testing.Mechanics.Car.Registration>> Register(
                    global::MooVC.Testing.Mechanics.Car.Register.Register register,
                    global::System.Threading.CancellationToken cancellationToken)
                {
                    _ = global::Ardalis.GuardClauses.Guard.Against.Null(register, message: "The request must be provided.");

                    using (manager.Begin(global::Mu.Auditing.Scope.Internal))
                    {
                        using (scribe.Next(register, out _))
                        {
                            return await mediator
                                .Execute<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>(
                                    register,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                }
            }
            """;

        var visitor = new GenerateServiceWhenFeatureVisited();

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .HasService(false)
            .HasServiceContract(hasServiceContract));

        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenAFeatureWhenHasServiceThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateServiceWhenFeatureVisited();
        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata.HasService(true));
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
        var visitor = new GenerateServiceWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}