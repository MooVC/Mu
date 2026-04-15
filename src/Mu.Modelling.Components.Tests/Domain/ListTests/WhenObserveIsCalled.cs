#if NET10_0_OR_GREATER
namespace Mu.Modelling.Components.Domain.ListTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;

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
            public sealed partial record Locations
            {
                [Description("The Front Left Wheel")]
                public static readonly Locations FrontLeft = nameof(FrontLeft);

                [Description("The Front Right Wheel")]
                public static readonly Locations FrontRight = nameof(FrontRight);

                [Description("The Rear Left Wheel")]
                public static readonly Locations RearLeft = nameof(RearLeft);

                [Description("The Rear Right Wheel")]
                public static readonly Locations RearRight = nameof(RearRight);

                private Locations(string value)
                {
                    _value = value;
                }

                public bool IsFrontLeft => this == FrontLeft;
 
                public bool IsFrontRight => this == FrontRight;

                public bool IsRearLeft => this == RearLeft;

                public bool IsRearRight => this == RearRight;

                public override string ToString()
                {
                    return _value;
                }
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
#endif