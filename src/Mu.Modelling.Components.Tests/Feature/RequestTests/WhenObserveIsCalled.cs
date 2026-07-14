namespace Mu.Modelling.Components.Feature.RequestTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAFeatureWhenCreationThenCreationalDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Request();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using System.ComponentModel;
            using Muify.Service;

            [Creational(Fact = "Registered")]
            [Description("Registers a Car within the Mechanics System")]
            public sealed partial record Register(byte Doors, string Make, string Model);
            """;

        var expected = new File(content, "cs", "Register", "src/MooVC.Testing.Mechanics.Car.Register/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Register, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAFeatureWhenNonMutationalThenNonMutationalDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Request();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car.FindCarsBy;

            using System;
            using System.ComponentModel;
            using Muify.Service;

            [Description("Finds Cars By Make and/or Model")]
            [NonMutational]
            public sealed partial record FindCarsBy(string? Make = default, string? Model = default);
            """;

        var expected = new File(content, "cs", "FindCarsBy", "src/MooVC.Testing.Mechanics.Car.FindCarsBy/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.FindCarsBy, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAFeatureWhenTransitionalThenTransitionalDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Request();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car.Unregister;

            using System.ComponentModel;
            using Muify.Service;

            [Description("Removes a Car from the Mechanics System")]
            [Transitional(Fact = "Unregistered")]
            public sealed partial record Unregister(Registration Identity);
            """;

        var expected = new File(content, "cs", "Unregister", "src/MooVC.Testing.Mechanics.Car.Unregister/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Unregister, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}