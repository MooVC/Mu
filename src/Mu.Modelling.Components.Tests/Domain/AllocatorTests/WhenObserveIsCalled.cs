namespace Mu.Modelling.Components.Domain.AllocatorTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using MooVC.Modelling;
using Mu.Modelling.Testing;

public sealed class WhenObserveIsCalled
{
    [Test]
    public async Task GivenAUnitThenAggregateDefinitionIsReturned()
    {
        // Arrange
        var visitor = new Allocator();

        const string content = """
            namespace MooVC.Testing.Mechanics.Car;

            using System.Threading;
            using System.Threading.Tasks;
            using Mu.Modelling.Behavior;
            using Mu.Modelling.Services;

            public sealed partial class Allocator
                : IAllocator<Registration>
            {
                public override ValueTask<Registration> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
                    where TUseCase : UseCase
                {
                    throw new NotImplementedException();
                }

                public override ValueTask Confirm(Registration identity, CancellationToken cancellationToken)
                {
                    return ValueTask.CompletedTask;
                }

                public override ValueTask Surrender(Registration identity, CancellationToken cancellationToken)
                {
                    return ValueTask.CompletedTask;
                }
            }
            """;

        var expected = new File(content, "cs", "Allocator", "src/MooVC.Testing.Mechanics.Car/");

        // Act
        IAsyncEnumerable<File> results = visitor.Observe(TestData.Single.Car, CancellationToken.None);

        // Assert
        _ = await Assert.That(results).HasCount(1);
        File item = await results.FirstAsync();
        _ = await Assert.That(item).IsEqualTo(expected);
    }
}