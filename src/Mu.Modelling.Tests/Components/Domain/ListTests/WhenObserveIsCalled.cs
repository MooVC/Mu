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
            using Muify.Domain;

            [Description("Represents the Location of the Wheel on the Car")]
            [Monify<string>]
            public sealed partial record Location
            {
                [Description("The Front Left Wheel")]
                public static readonly Location FrontLeft = "FrontLeft";

                [Description("The Front Right Wheel")]
                public static readonly Location FrontRight = "FrontRight";

                [Description("The Rear Left Wheel")]
                public static readonly Location RearLeft = "RearLeft";

                [Description("The Rear Right Wheel")]
                public static readonly Location RearRight = "RearRight";

                private Location(string value)
                {
                    _value = value;
                }

                public bool IsFrontLeft => this == FrontLeft;
 
                public bool IsFrontRight => this == IsFrontRight;

                public bool IsRearLeft => this == IsRearLeft;

                public bool IsRearRight => this == IsRearRight;

                public override string ToString()
                {
                    return _value;
                }
            }
            """;

        var expected = new File(content, "cs", "Location", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Location, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}
#endif