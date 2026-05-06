namespace Mu.Modelling.Components.Domain.ValueTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAValueWhenWithinAnAreaThenValueDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Value();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using System.ComponentModel;

            [Description("Represents a Pressure Measurement Associated with a Wheel")]
            public sealed partial record Pressure(
                [Description("The Unit of Measurement Associated with the Pressure")] Unit Unit,
                [Description("The Value Associated with the Pressure based on the Unit")] decimal Value);
            """;

        var expected = new File(content, "cs", "Pressure", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Pressure, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}