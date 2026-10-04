namespace Muify.Domain.GenerateRegistrarWhenUnitVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAPartialUnitWhenHasRegistrarIsFalseThenRegistrarDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            using SimpleInjector;

            partial record Car
                : global::Mu.Composition.IRegistrar
            {
                public static void Register(global::Microsoft.Extensions.Configuration.IConfiguration configuration, global::SimpleInjector.Container container)
                {
                    _ = global::MooVC.Testing.Mechanics.Car.Car.Bind(global::ProtoBuf.Meta.RuntimeTypeModel.Default);
                }
            }
            """;

        var visitor = new GenerateRegistrarWhenUnitVisited();
        Model model = TestData.Single.Model;

        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasRegistrar(false));

        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Car.Registrar");
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenOwnedComponentsWhenBindersAreGeneratedOrExistingThenTheyAreAppliedBeforeCustomRegistrars(bool hasBinder)
    {
        // Arrange
        const string expectedBinding = "_ = global::MooVC.Testing.Mechanics.Car.Wheel.Bind(global::ProtoBuf.Meta.RuntimeTypeModel.Default);";
        const string expectedRegistrar = "global::MooVC.Testing.Mechanics.Car.Allocator.Register(configuration, container);";
        const string excludedBinding = "global::MooVC.Testing.Mechanics.Car.Pressure.Bind";
        var visitor = new GenerateRegistrarWhenUnitVisited();

        Component wheel = TestData.Single.Wheel.Value.WithMetadata(metadata => metadata
            .HasBinder(hasBinder)
            .WithCharacteristics(characteristics => characteristics.IsClass(true)));

        Unit car = TestData.Single.Car.Value
            .Owns(wheel)
            .WithMetadata(metadata => metadata
                .IsPartial(true)
                .HasRegistrar(false)
                .WithRegistrars((Name: "Allocator", Qualifier: TestData.Single.Car.Namespace)));

        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).Contains(expectedBinding);
        _ = await Assert.That(definition.Content.Contains(excludedBinding, StringComparison.Ordinal)).IsFalse();
        _ = await Assert.That(definition.Content.IndexOf(expectedBinding, StringComparison.Ordinal))
            .IsLessThan(definition.Content.IndexOf(expectedRegistrar, StringComparison.Ordinal));
    }

    [Test]
    public async Task GivenAPartialUnitWhenHasRegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegistrarWhenUnitVisited();
        Model model = TestData.Single.Model;

        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata
            .IsPartial(true)
            .HasRegistrar(true));

        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenHasRegistrarIsFalseThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegistrarWhenUnitVisited();
        Model model = TestData.Single.Model;

        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata
            .IsPartial(false)
            .HasRegistrar(false));

        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateRegistrarWhenUnitVisited();
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, TestData.Single.Car.Value);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}