namespace Mu.Modelling.Services;

using System;
using Mu.Modelling.Behavior;

/// <summary>
/// An allocator that generates GUIDs for use cases.
/// </summary>
public sealed class GuidAllocator
    : IAllocator<Guid>
{
    /// <inheritdoc />
    public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
        where TUseCase : UseCase
    {
        var identifier = Guid.CreateVersion7(useCase.Proposed);

        return ValueTask.FromResult(identifier);
    }

    public ValueTask Confirm<TUseCase>(Guid identity, CancellationToken cancellationToken)
        where TUseCase : UseCase
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Surrender<TUseCase>(Guid identity, CancellationToken cancellationToken)
        where TUseCase : UseCase
    {
        return ValueTask.CompletedTask;
    }
}