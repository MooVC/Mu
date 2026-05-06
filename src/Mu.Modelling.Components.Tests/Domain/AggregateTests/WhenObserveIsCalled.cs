namespace Mu.Modelling.Components.Domain.AggregateTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitThenAggregateDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Aggregate();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using System.Collections.Immutable;
            using System.ComponentModel;
            using Muify.Domain;

            [Description("Represents a Vehicle that has utilizes the services of the Mechanics")]
            [Unit<Guid>]
            public sealed partial record Car(
                [Description("The Number of Passenger Doors")] byte Doors,
                [Description("The Manufacturer of the Car")] string Make,
                [Description("The Manufacturer Ascribed Name")] string Model,
                [Description("The Wheels Attached to the Car")] ImmutableArray<Wheel> Wheels);
            """;

        var expected = new File(content, "cs", "Car", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Car, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}