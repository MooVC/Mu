using Mu.Communications.Messaging;

namespace Mu.Communications.Mediation;

using Mu.Modelling.Behavior;

public interface IHandler<TUseCase, TResult>
    where TUseCase : UseCase
    where TResult : notnull
{
    Task<Outcome<TResult>> Handle(Intent<TUseCase> intent, CancellationToken cancellationToken);
}