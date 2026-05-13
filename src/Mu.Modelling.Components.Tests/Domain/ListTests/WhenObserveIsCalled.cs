namespace Mu.Modelling.Components.Domain.ListTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;
using Mu.Modelling.Testing;
using List = Mu.Modelling.Components.Domain.List;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAListWhenWithinAUnitThenListDefinitionIsReturned()
    {
        // Arrange
        var visitor = new List();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using System.ComponentModel;
            using Monify;

            [Description("Represents the Location of the Wheel on the Car")]
            [Monify<string>]
            public readonly partial record struct Locations
            {
                [Description("The Front Left Wheel")]
                public static readonly Locations FrontLeft = $"{nameof(FrontLeft)}";

                [Description("The Front Right Wheel")]
                public static readonly Locations FrontRight = $"{nameof(FrontRight)}";

                [Description("The Rear Left Wheel")]
                public static readonly Locations RearLeft = $"{nameof(RearLeft)}";

                [Description("The Rear Right Wheel")]
                public static readonly Locations RearRight = $"{nameof(RearRight)}";
            }
            """;

        var expected = new File(content, "cs", "Locations", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Location, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}