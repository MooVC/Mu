namespace Mu.Services;

using System;
using Mu.Modelling.Behavior;

public sealed class GuidAllocator
    : IAllocator<Guid>
{
    public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
        where TUseCase : UseCase
    {
        var identifier = Guid.CreateVersion7(useCase.Proposed);

        return ValueTask.FromResult(identifier);
    }
}