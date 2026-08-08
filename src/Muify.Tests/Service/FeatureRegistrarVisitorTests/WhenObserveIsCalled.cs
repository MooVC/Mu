namespace Muify.Service.FeatureRegistrarVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureWhenHasRegistrarIsFalseThenRegistrarDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using SimpleInjector;

            partial record Register
                : global::Mu.Composition.IRegistrar
            {
                public static void Register(global::Microsoft.Extensions.Configuration.IConfiguration configuration, global::SimpleInjector.Container container)
                {
                    container.Register<global::Mu.Communications.Mediation.IHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Communications.Mediation.ServiceHandler<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>>(global::SimpleInjector.Lifestyle.Scoped);
                    container.Register<global::Mu.Modelling.Services.IService<global::MooVC.Testing.Mechanics.Car.Register.Register, global::MooVC.Testing.Mechanics.Car.Registration>, global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::MooVC.Testing.Mechanics.Car.Registration, global::MooVC.Testing.Mechanics.Car.Register.Register>>(global::SimpleInjector.Lifestyle.Scoped);
                }
            }
            """;

        var visitor = new FeatureRegistrarVisitor();
        Model model = TestData.Single.Model;
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasRegistrar(false));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Register.Registrar");
    }

    [Test]
    public async Task GivenAFeatureWhenHasRegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureRegistrarVisitor();
        Model model = TestData.Single.Model;
        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata.HasRegistrar(true));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, model, register);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new FeatureRegistrarVisitor();
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = new(TestData.Single.Features, 1, model, TestData.Single.Register.Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}