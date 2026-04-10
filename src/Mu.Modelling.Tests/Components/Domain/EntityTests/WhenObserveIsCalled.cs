#if NET10_0_OR_GREATER
namespace Mu.Modelling.Components.Domain.EntityTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAnEntityWhenWithinAUnitThenEntityDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Entity();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System.ComponentModel;
            using Muify.Domain;

            [Description("Represents a Wheel Attached to the Car")]
            public sealed partial class Wheel
            {
                [Description("The Location of the Wheel on the Car")]
                [Identity]
                public Location Location { get; init; }

                [Description("The Pressure of the Tyre on the Wheel")]
                public Pressure Pressure { get; init; }
            }
            """;

        var expected = new File(content, "cs", "Wheel", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Wheel, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}
#endif