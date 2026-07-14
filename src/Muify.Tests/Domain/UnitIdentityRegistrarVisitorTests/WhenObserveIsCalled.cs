namespace Muify.Domain.UnitIdentityRegistrarVisitorTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitWhenHasRegistrarIsFalseThenRegistrarDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using SimpleInjector;

            public sealed partial class Allocator
                : global::Mu.Composition.IRegistrar
            {
                public static void Register(global::Microsoft.Extensions.Configuration.IConfiguration configuration, global::SimpleInjector.Container container)
                {
                    container.RegisterConditional<global::Mu.Modelling.Services.IAllocator<global::MooVC.Testing.Mechanics.Car.Registration>, Allocator>(Lifestyle.Scoped, context => context.Consumer.ImplementationType.Namespace.StartsWith("My.Namespace", StringComparison.Ordinal));
                }
            }
            """;

        var visitor = new UnitIdentityRegistrarVisitor();
        Unit car = TestData.Single.Car.Value.WithMetadata(metadata => metadata.WithAllocator(allocator => allocator.HasRegistrar(false)));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);
        Model.Graph.Areas.Area.Units.Unit.Identity identity = new(unit, TestData.Single.Model, unit.Value.Identity);

        // Act
        IEnumerable<File> result = visitor.Observe(identity);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo("Allocator.Registrar");
    }

    [Test]
    public async Task GivenAUnitWhenHasRegistrarThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitIdentityRegistrarVisitor();
        Unit car = TestData.Single.Car.Value.WithMetadata(metadata => metadata.WithAllocator(allocator => allocator.HasRegistrar(true)));
        Model.Graph.Areas.Area.Units.Unit unit = new(TestData.Single.Units, 0, TestData.Single.Model, car);
        Model.Graph.Areas.Area.Units.Unit.Identity identity = new(unit, TestData.Single.Model, unit.Value.Identity);

        // Act
        IEnumerable<File> result = visitor.Observe(identity);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAFeatureWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new UnitIdentityRegistrarVisitor();

        // Act
        IEnumerable<File> result = visitor.Observe(TestData.Single.Identity);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}