namespace Mu.Communications.Mediation;

using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;

/// <summary>
/// Adapts an <see cref="IService{TUseCase, TResult}"/> to the mediation handler contract.
/// </summary>
public sealed class ServiceHandler<TUseCase, TResult>(IService<TUseCase, TResult> service)
    : IHandler<TUseCase, TResult>
    where TUseCase : UseCase
    where TResult : notnull
{
    /// <summary>
    /// Handles an intent by delegating execution to the underlying service.
    /// </summary>
    public async Task<Outcome<TResult>> Handle(Intent<TUseCase> intent, CancellationToken cancellationToken)
    {
        Result<TResult> result = await service
            .Execute(intent.UseCase, cancellationToken)
            .ConfigureAwait(false);

        return intent.Yields(result);
    }
}