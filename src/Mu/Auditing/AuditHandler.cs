namespace Mu.Auditing;

using System;
using Mu.Auditing.Configuration;
using Mu.Communications.Mediation;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;

public sealed class AuditHandler<TUseCase, TResult>(IAuditor auditor, AuditOptions options, IAuditScopeManager manager, IHandler<TUseCase, TResult> next)
    : IHandler<TUseCase, TResult>
    where TUseCase : UseCase
    where TResult : notnull
{
    public async Task<Outcome<TResult>> Handle(Intent<TUseCase> intent, CancellationToken cancellationToken)
    {
        if (ShouldAudit(intent))
        {
            return await HandleWithAuditing(intent, cancellationToken);
        }

        return await next
            .Handle(intent, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Outcome<TResult>> HandleWithAuditing(Intent<TUseCase> intent, CancellationToken cancellationToken)
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

    private bool ShouldAudit(TUseCase useCase)
    {
        AuditOperationScope operation = useCase is Mutational
            ? options.Mutational
            : options.NonMutational;

        return manager.Scope == AuditScope.External || operation == AuditOperationScope.All;
    }
}