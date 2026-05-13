namespace Mu.Modelling.Components.Feature.ResultTests;

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
        var visitor = new Result();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using System.ComponentModel;

            public sealed partial record Register
            {
                public sealed partial record Result([Description("The Identity of the Newly Created Car")] Guid Identity);
            }
            """;

        var expected = new File(content, "cs", "Register.Result", "src/MooVC.Testing.Mechanics.Car.Register/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Register, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = expected.ToString();
        _ = await Assert.That(item).IsEqualTo(expected);
    }

    [Test]
    public async Task GivenAFeatureWhenNonMutationalThenNonMutationalDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Result();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car.FindCarsBy;

            using System.Collections.Immutable;
            using MooVC.Testing.Mechanics.Car;

            public sealed partial record FindCarsBy
            {
                public sealed partial record Result(ImmutableArray<Car> Cars);
            }
            """;

        var expected = new File(content, "cs", "FindCarsBy.Result", "src/MooVC.Testing.Mechanics.Car.FindCarsBy/");

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
        var visitor = new Result();

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Unregister, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).IsEmpty();
    }
}