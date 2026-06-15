namespace Mu.Communications.Mediation;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mu.Communications.Messaging;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;

/// <summary>
/// Resolves and executes handlers for use cases through dependency injection.
/// </summary>
public sealed partial class Mediator(ILogger<Mediator> logger, IServiceProvider provider)
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

        Type useCaseType = typeof(TUseCase);

        LogExecutionRequested(logger, useCaseType, useCase.Identity);

        try
        {
            IHandler<TUseCase, TResult> handler = provider.GetRequiredService<IHandler<TUseCase, TResult>>();

            var intent = new Intent<TUseCase>(scope.Ledger, useCase);

            Outcome<TResult> outcome = await handler
                .Handle(intent, cancellationToken)
                .ConfigureAwait(false);

            LogExecutionSucceeded(logger, useCaseType, useCase.Identity);

            return outcome;
        }
        catch (Exception exception)
        {
            LogExecutionFailed(logger, useCaseType, useCase.Identity, exception);

            throw;
        }
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to execute `{UseCaseType}`: `{UseCaseId}`")]
    private static partial void LogExecutionFailed(ILogger logger, Type useCaseType, Guid useCaseId, Exception exception);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Executing `{UseCaseType}`: `{UseCaseId}`")]
    private static partial void LogExecutionRequested(ILogger logger, Type useCaseType, Guid useCaseId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Successfully executed `{UseCaseType}`: `{UseCaseId}`")]
    private static partial void LogExecutionSucceeded(ILogger logger, Type useCaseType, Guid useCaseId);
}