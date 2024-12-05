namespace Mu.Architecture.Communication.Mediation;

public interface IMediator
{
    Task<Result<TResult>> Execute<TUseCase, TResult>(TUseCase useCase, CancellationToken cancellationToken);
}