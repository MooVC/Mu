namespace Mu.Services;

public interface IService<in TUseCase, TResult>
{
    Task<TResult> Execute(TUseCase useCase, CancellationToken cancellationToken);
}