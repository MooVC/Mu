namespace Muify.Domain.GenerateIdentifierImplicitConversionWhenComponentVisitedTests;

using Mu.Modelling;
using Mu.Modelling.Testing;
using Muify;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAComponentWhenIdentifierHasImplicitConversionIsFalseThenImplicitConversionToIdentifierDefinitionIsGenerated()
    {
        // Arrange
        const string expected = """
            namespace MooVC.Testing.Mechanics.Car;

            using Ardalis.GuardClauses;

            partial class Wheel
            {
                public static implicit operator Locations(Wheel subject)
                {
                    _ = Guard.Against.Null(subject, message: "The subject to convert to its identifier must be provided.");

                    return subject.Location;
                }
            }
            """;

        var visitor = new GenerateIdentifierImplicitConversionWhenComponentVisited();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier.HasImplicitConversion(false)));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        File definition = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(definition.Content).IsEqualTo(expected);
        _ = await Assert.That(definition.Hint).IsEqualTo($"{wheel.Name}.Identifier.Conversion.Implicit");
    }

    [Test]
    public async Task GivenAComponentWhenIdentifierHasImplicitConversionThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateIdentifierImplicitConversionWhenComponentVisited();

        Component wheel = TestData.Single.Units.Value[0].Components[1]
            .WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier.HasImplicitConversion(true)));

        Model.Graph.Areas.Area.Units.Unit.Components.Component component = new(TestData.Single.Components, 0, TestData.Single.Model, wheel);

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GivenAComponentWhenOutOfScopeThenNothingIsGenerated()
    {
        // Arrange
        var visitor = new GenerateIdentifierImplicitConversionWhenComponentVisited();
        Model.Graph.Areas.Area.Units.Unit.Components.Component component = TestData.Single.Wheel;

        // Act
        IEnumerable<File> result = visitor.Observe(component);

        // Assert
        _ = await Assert.That(result).IsEmpty();
    }
}