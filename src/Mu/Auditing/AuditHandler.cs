namespace Mu.Auditing;

using System;
using Mu.Communications.Mediation;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;

public sealed class AuditHandler<TUseCase, TResult>(IAuditor auditor, IHandler<TUseCase, TResult> next)
    : IHandler<TUseCase, TResult>
    where TUseCase : UseCase
    where TResult : notnull
{
    public async Task<Outcome<TResult>> Handle(Intent<TUseCase> intent, CancellationToken cancellationToken)
    {
        Guid identity = Guid.Empty;

        try
        {
            identity = await auditor
                .Capture(intent, cancellationToken)
                .ConfigureAwait(false);

            Outcome<TResult> result = await next
                .Handle(intent, cancellationToken)
                .ConfigureAwait(false);

            await auditor.Complete(identity, result, cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
        catch (Exception exception)
        {
            if (identity != Guid.Empty)
            {
                await auditor
                    .Fail(exception, identity, cancellationToken)
                    .ConfigureAwait(false);
            }

            throw;
        }
    }
}