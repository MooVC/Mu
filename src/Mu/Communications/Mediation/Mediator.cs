namespace Mu.Communications.Mediation;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mu.Communications.Messaging;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;

/// <summary>
/// Resolves and executes handlers for use cases through dependency injection.
/// </summary>
public sealed class Mediator(IServiceProvider provider)
    : IMediator
{
    /// <summary>
    /// Executes a use case by resolving and invoking the matching handler.
    /// </summary>
    public async Task<Result<TResult>> Execute<TUseCase, TResult>(TUseCase useCase, CancellationToken cancellationToken)
        where TUseCase : UseCase
        where TResult : notnull
    {
        using var scope = new Scope(useCase);

        IHandler<TUseCase, TResult> handler = provider.GetRequiredService<IHandler<TUseCase, TResult>>();

        var intent = new Intent<TUseCase>(scope.Ledger, useCase);

        return await handler
            .Handle(intent, cancellationToken)
            .ConfigureAwait(false);
    }
}
