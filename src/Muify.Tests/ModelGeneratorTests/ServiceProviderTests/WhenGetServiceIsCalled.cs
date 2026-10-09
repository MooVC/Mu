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
        const string expectedHint = "MooVC.Testing.Mechanics.Wheel.Binder";
        var provider = new ModelGenerator.ServiceProvider();
        Component wheel = TestData.Single.Wheel._Value.WithMetadata(metadata => metadata.HasBinder(false));
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
            "MooVC.Testing.Mechanics.Car.Register.IRegisterService",
            "MooVC.Testing.Mechanics.Car.Register.IRegisterService.Grpc",
            "MooVC.Testing.Mechanics.Car.Register.Register",
            "MooVC.Testing.Mechanics.Car.Register.Register.Binder",
            "MooVC.Testing.Mechanics.Car.Register.Register.ctor",
            "MooVC.Testing.Mechanics.Car.Register.Register.Registrar",
            "MooVC.Testing.Mechanics.Car.Register.Registered",
            "MooVC.Testing.Mechanics.Car.Register.RegisterOptions",
            "MooVC.Testing.Mechanics.Car.Register.RegisterService",
            "MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Client",
            "MooVC.Testing.Mechanics.Car.Register.RegisterService.Grpc.Service",
            "MooVC.Testing.Mechanics.Car.Register.Transform",
        ];

        var provider = new ModelGenerator.ServiceProvider();

        Feature register = TestData.Single.Register._Value.WithMetadata(metadata => metadata
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
        const string expectedHint = "MooVC.Testing.Mechanics.Car.Wheel.Binder";
        var provider = new ModelGenerator.ServiceProvider();
        Component wheel = TestData.Single.Wheel._Value.WithMetadata(metadata => metadata.HasBinder(false));
        var component = new UnitComponent(TestData.Single.Components, 1, TestData.Single.Model, wheel);

        // Act
        var visitors = (IEnumerable<IModelVisitor<UnitComponent, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<UnitComponent, File>>));
        IEnumerable<File> files = visitors.SelectMany(visitor => visitor.Observe(component));

        // Assert
        File binder = await Assert.That(files).HasSingleItem();
        _ = await Assert.That(binder.Hint).IsEqualTo(expectedHint);
    }

    [Test]
    public async Task GivenComponentsWithTheSameNameInDifferentNamespacesThenTheirBindersHaveDistinctHints()
    {
        // Arrange
        string[] expectedHints =
        [
            "MooVC.Testing.Mechanics.Wheel.Binder",
            "MooVC.Testing.Mechanics.Car.Wheel.Binder",
        ];

        var provider = new ModelGenerator.ServiceProvider();
        Component wheel = TestData.Single.Wheel._Value.WithMetadata(metadata => metadata.HasBinder(false));
        var components = new AreaComponents(TestData.Single.Mechanics, TestData.Single.Model, [wheel]);
        var areaComponent = new AreaComponent(components, 0, TestData.Single.Model, wheel);
        var unitComponent = new UnitComponent(TestData.Single.Components, 1, TestData.Single.Model, wheel);
        var areaVisitors = (IEnumerable<IModelVisitor<AreaComponent, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<AreaComponent, File>>));
        var unitVisitors = (IEnumerable<IModelVisitor<UnitComponent, File>>)provider.GetService(typeof(IEnumerable<IModelVisitor<UnitComponent, File>>));

        // Act
        string[] hints = areaVisitors
            .SelectMany(visitor => visitor.Observe(areaComponent))
            .Concat(unitVisitors.SelectMany(visitor => visitor.Observe(unitComponent)))
            .Select(file => file.Hint)
            .ToArray();

        // Assert
        _ = await Assert.That(hints).IsEquivalentTo(expectedHints);
        _ = await Assert.That(hints.Distinct().Count()).IsEqualTo(hints.Length);
    }
}