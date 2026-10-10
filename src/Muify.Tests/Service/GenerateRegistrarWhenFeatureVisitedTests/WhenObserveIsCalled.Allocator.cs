namespace Muify.Service.GenerateRegistrarWhenFeatureVisitedTests;

using MooVC.Syntax.CSharp;
using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed partial class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAGuidCreationalFeatureWhenNoCustomAllocatorExistsThenItsServiceGetsAConditionalAllocator()
    {
        // Arrange
        const string expected = """
            container.RegisterConditional<global::Mu.Modelling.Services.IAllocator<global::System.Guid>, global::Mu.Modelling.Services.GuidAllocator>(
                        global::SimpleInjector.Lifestyle.Scoped,
                        context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof(global::Mu.Modelling.Services.CreationalService<global::MooVC.Testing.Mechanics.Car.Car, global::System.Guid, global::MooVC.Testing.Mechanics.Car.Register.Register>));
            """;
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).Contains(expected);
    }

    [Test]
    public async Task GivenACustomAllocatorThenNoDefaultAllocatorIsRegistered()
    {
        // Arrange
        const string defaultAllocator = "GuidAllocator";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value
            .IdentifiedBy((Qualification)typeof(Guid))
            .WithMetadata(metadata => metadata.WithAllocator(allocator => allocator
                .WithDefinition((Name: "Allocator", Qualifier: TestData.Single.Car.Namespace))));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content.Contains(defaultAllocator, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    [Arguments("Int64", "System")]
    [Arguments("Guid", "Custom")]
    public async Task GivenANonGuidIdentityThenNoDefaultAllocatorIsRegistered(string name, string qualifier)
    {
        // Arrange
        const string defaultAllocator = "GuidAllocator";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)(name, qualifier));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content.Contains(defaultAllocator, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task GivenAComponentIdentityThenNoDefaultAllocatorIsRegistered()
    {
        // Arrange
        const string defaultAllocator = "GuidAllocator";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(TestData.Single.Car._Value, TestData.Single.Register._Value);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content.Contains(defaultAllocator, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenATransitionalOrQueryFeatureThenNoDefaultAllocatorIsRegistered(bool isQuery)
    {
        // Arrange
        const string defaultAllocator = "GuidAllocator";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Feature request = isQuery ? TestData.Single.FindCarsBy._Value : TestData.Single.Unregister._Value;
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, request);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content.Contains(defaultAllocator, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenACustomServiceOrHandlerThenNoDefaultAllocatorIsRegistered(bool hasCustomHandler)
    {
        // Arrange
        const string defaultAllocator = "GuidAllocator";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Feature request = TestData.Single.Register._Value.WithMetadata(metadata => hasCustomHandler
            ? metadata.WithHandler((Name: "Handler", Qualifier: TestData.Single.Register.Namespace))
            : metadata.WithService((Name: "Service", Qualifier: TestData.Single.Register.Namespace)));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, request);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content.Contains(defaultAllocator, StringComparison.Ordinal)).IsFalse();
    }

    [Test]
    public async Task GivenACustomRegistrarThenItRunsBeforeTheDefaultAllocatorRegistration()
    {
        // Arrange
        const string customRegistrar = "global::MooVC.Testing.Mechanics.Car.Register.CustomRegistrar.Register(configuration, container);";
        const string defaultAllocator = "container.RegisterConditional<";
        const string serviceRegistration = "container.Register<global::Mu.Modelling.Services.IService<";
        var visitor = new GenerateRegistrarWhenFeatureVisited();
        Unit car = TestData.Single.Car._Value.IdentifiedBy((Qualification)typeof(Guid));
        Feature request = TestData.Single.Register._Value.WithMetadata(metadata => metadata
            .WithRegistrars((Name: "CustomRegistrar", Qualifier: TestData.Single.Register.Namespace)));
        Model.Graph.Areas.Area.Units.Unit.Features.Feature feature = DefineAllocatorFeature(car, request);

        // Act
        IEnumerable<File> result = visitor.Observe(feature);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).Contains(customRegistrar);
        _ = await Assert.That(definition.Content.IndexOf(serviceRegistration, StringComparison.Ordinal))
            .IsLessThan(definition.Content.IndexOf(customRegistrar, StringComparison.Ordinal));
        _ = await Assert.That(definition.Content.IndexOf(defaultAllocator, StringComparison.Ordinal))
            .IsGreaterThan(definition.Content.IndexOf(customRegistrar, StringComparison.Ordinal));
    }

    private static Model.Graph.Areas.Area.Units.Unit.Features.Feature DefineAllocatorFeature(Unit car, Feature request)
    {
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);
        Model.Graph.Areas.Area.Units.Unit.Features features = new(unit, model, car.Features);
        Feature definition = request.WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasRegistrar(false));

        return new(features, 0, model, definition);
    }
}