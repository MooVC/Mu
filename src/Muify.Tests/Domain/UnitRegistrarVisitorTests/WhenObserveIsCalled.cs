namespace Muify.Domain.UnitRegistrarVisitorTests;

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
                    // There are no registrars defines within the assembly
                }
            }
            """;

        var visitor = new UnitRegistrarVisitor();
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
        _ = await Assert.That(definition.Hint).IsEqualTo("Registrar");
    }

    [Test]
    public async Task GivenAPartialUnitWhenHasRegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitRegistrarVisitor();
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
        var visitor = new UnitRegistrarVisitor();
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
        var visitor = new UnitRegistrarVisitor();
        Model model = TestData.Single.Model;
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, model, TestData.Single.Car.Value);

        // Act
        IEnumerable<File> result = visitor.Observe(unit);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}