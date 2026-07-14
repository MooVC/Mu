namespace Mu.Modelling.Components.Domain.IdentityTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAValueWhenWithinAnAreaThenValueDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Identity();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using System.ComponentModel;

            [Description("Represents a Registration for a Car")]
            public readonly partial record struct Registration
            {
                [Description("The unique number attributed to the Car")]
                public string Number { get; init; }
            }
            """;

        var expected = new File(content, "cs", "Registration", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Identity, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}