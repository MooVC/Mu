namespace Muify.Service.FeatureServiceVisitorTests;

using System;
using System.Collections.Generic;
using System.Text;
using Mu.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureWhenHasServiceContractIsFalseThenServiceContractIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            public sealed partial class RegisterService(
                global::Mu.Communications.Mediation.IHandler<Register, Register.Result> handler,
                global::Mu.Auditing.IScopeManager manager,
                global::Mu.Communications.Tracing.IScribe scribe)
                : IRegisterService
            {
                public async global::System.Threading.Tasks.Task<Register.Result> Register(Register register, global::System.Threading.CancellationToken cancellationToken)
                {
                    using (manager.Begin(global::Mu.Auditing.Scope.Internal));
                    using (scribe.Next(register));

                    return await handler
                        .Handle(register, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            """;

        var visitor = new FeatureServiceVisitor();
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasService(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo(register.Name);
    }

    [Test]
    public async Task GivenFeatureWhenHasServiceContractThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureServiceVisitor();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasService(true));
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
        var visitor = new FeatureServiceVisitor();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = TestData.Single.Register;

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}