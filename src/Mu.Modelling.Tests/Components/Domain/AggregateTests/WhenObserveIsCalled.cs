#if NET10_0_OR_GREATER
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
        const int index = 0;
        var visitor = new Aggregate();

        var graph = new Model.Graph.Areas.Area.Units.Unit(
            index,
            TestData.Single,
            TestData.Single.Areas,
            TestData.Single.Areas[0],
            TestData.Single.Areas[0].Units[0]);

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using System.ComponentModel;
            using Mu.Modelling.State;

            [Description("Represents a Vehicle that has utilizes the services of the Mechanics")]
            public sealed partial record Car(
                [Description("The Number of Passenger Doors")] byte Doors,
                [Description("The Manufacturer of the Car")] string Make,
                [Description("The Name Ascribed to the Car by the Manufacturer")] string Model)
                : Aggregate
            {
            }
            """;

        var expected = new File(content, "cs", "Car", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(graph, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}
#endif