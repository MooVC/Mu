namespace Muify.Domain.UnitRegistrarVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitWhenHasRegistrarIsFalseThenRegistrarDefinitionIsGenerated()
    {
        // Arrange
        string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            using SimpleInjector;

            public sealed partial record Car
                : global::Mu.Composition.IRegistrar
            {
                public static global::SimpleInjector.Container Register(
                    global::Microsoft.Extensions.Configuration.IConfiguration configuration,
                    global::SimpleInjector.Container container)
                {
                    return container;
                }
            }
            """;

        var visitor = new UnitRegistrarVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasRegistrar(false));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Registrar");
    }

    [Test]
    public async Task GivenAUnitWhenHasRegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitRegistrarVisitor();
        Unit car = TestData.Single.Units.Value[0].WithMetadata(metadata => metadata.HasRegistrar(true));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAUnitWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitRegistrarVisitor();
        Model.Graph.Areas.Area.Units.Unit unit = TestData.Single.Car;

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}