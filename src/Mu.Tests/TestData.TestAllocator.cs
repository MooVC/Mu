namespace Mu.Testing;

using System.Diagnostics.CodeAnalysis;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;

public static partial class TestData
{
    public sealed class TestAllocator
        : IAllocator<Guid>
    {
        public IList<Guid> Confirmed { get; } = [];

        [SuppressMessage("Major Code Smell", "S3218:Inner class members should not shadow outer class static or type members", Justification = "The property names the identity allocated by this test double.")]
        public Guid Identity { get; init; } = TestData.Identity;

        public IList<UseCase> Requests { get; } = [];

        public IList<Guid> Surrendered { get; } = [];

        public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
            where TUseCase : UseCase
        {
            Requests.Add(useCase);

            return ValueTask.FromResult(Identity);
        }

        public ValueTask Confirm(Guid identity, CancellationToken cancellationToken)
        {
            Confirmed.Add(identity);

            return ValueTask.CompletedTask;
        }

        public ValueTask Surrender(Guid identity, CancellationToken cancellationToken)
        {
            Surrendered.Add(identity);

            return ValueTask.CompletedTask;
        }
    }
}