namespace Muify.ModelGeneratorTests.ServiceProviderTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using AreaComponent = Mu.Modelling.Model.Graph.Areas.Area.Components.Component;
using AreaComponents = Mu.Modelling.Model.Graph.Areas.Area.Components;
using FeatureGraph = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features.Feature;
using UnitComponent = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Components.Component;

public sealed class WhenGetServiceIsCalled
{
    [Test]
    public async Task GivenAnAreaComponentThenItsBinderIsGenerated()
    {
        // Arrange
        const string expectedHint = "Wheel.Binder";
        var provider = new ModelGenerator.ServiceProvider();
        Component wheel = TestData.Single.Wheel.Value.WithMetadata(metadata => metadata.HasBinder(false));
        var components = new AreaComponents(TestData.Single.Mechanics, TestData.Single.Model, [wheel]);
        var component = new AreaComponent(components, 0, TestData.Single.Model, wheel);

        // Act
        var visitors = (IEnumerable<IModelVisitor<AreaComponent, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<AreaComponent, File>>));
        IEnumerable<File> files = visitors.SelectMany(visitor => visitor.Observe(component));

        // Assert
        File binder = await Assert.That(files).HasSingleItem();
        _ = await Assert.That(binder.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenAFeatureWithoutGeneratedArtifactsThenAllFeatureVisitorsProduceDistinctFiles()
    {
        // Arrange
        string[] expectedHints =
        [
            "IRegisterService",
            "IRegisterService.Grpc",
            "Register",
            "Register.Binder",
            "Register.ctor",
            "Register.Registrar",
            "Registered",
            "RegisterService",
            "RegisterService.Grpc.Client",
            "RegisterService.Grpc.Service",
            "Transform",
        ];

        var provider = new ModelGenerator.ServiceProvider();

        Feature register = TestData.Single.Register.Value.WithMetadata(metadata => metadata
            .HasBase(false)
            .HasBinder(false)
            .HasConstructors(false)
            .HasFact(false)
            .HasGrpcClient(false)
            .HasGrpcService(false)
            .HasGrpcServiceContract(false)
            .HasRegistrar(false)
            .HasService(false)
            .HasServiceContract(false)
            .IsPartial(true));

        var feature = new FeatureGraph(TestData.Single.Features, 1, TestData.Single.Model, register);

        // Act
        var visitors = (IEnumerable<IModelVisitor<FeatureGraph, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<FeatureGraph, File>>));
        string[] hints = visitors.SelectMany(visitor => visitor.Observe(feature)).Select(file => file.Hint).ToArray();

        // Assert
        _ = await Assert.That(hints).IsEquivalentTo(expectedHints);
        _ = await Assert.That(hints.Distinct().Count()).IsEqualTo(hints.Length);
    }

    [Test]
    public async Task GivenAUnitComponentThenItsBinderIsGenerated()
    {
        // Arrange
        const string expectedHint = "Wheel.Binder";
        var provider = new ModelGenerator.ServiceProvider();
        Component wheel = TestData.Single.Wheel.Value.WithMetadata(metadata => metadata.HasBinder(false));
        var component = new UnitComponent(TestData.Single.Components, 1, TestData.Single.Model, wheel);

        // Act
        var visitors = (IEnumerable<IModelVisitor<UnitComponent, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<UnitComponent, File>>));
        IEnumerable<File> files = visitors.SelectMany(visitor => visitor.Observe(component));

        // Assert
        File binder = await Assert.That(files).HasSingleItem();
        _ = await Assert.That(binder.Hint).IsEqualTo(expectedHint);
    }
}