namespace Mu.Communications.Tracing;

using System;
using Ardalis.GuardClauses;
using Grpc.Core;
using Mu.Modelling.Behavior;
using ProtoBuf.Grpc;
using static Mu.Communications.Tracing.CallContextExtensions_Resources;

public static partial class CallContextExtensions
{
    public const string CausationHeader = "causation";
    public const string CorrelationHeader = "correlation";

    extension(CallContext context)
    {
        public Ledger ToLedger(UseCase useCase)
        {
            _ = Guard.Against.Null(useCase, message: ToLedgerUseCaseRequired);

            Metadata? headers = context.ServerCallContext?.RequestHeaders;

            if (headers is null)
            {
                return new Ledger(useCase.Identity);
            }

            string? correlationValue = headers?.GetValue(CorrelationHeader);
            string? causationValue = headers?.GetValue(CausationHeader);

            if (!TryParseLedgerIdentifiers(causationValue, correlationValue, out Guid causationId, out Guid correlationId))
            {
                return new Ledger(useCase.Identity);
            }

            return new Ledger(causationId, correlationId);
        }
    }

    private static bool TryParseLedgerIdentifiers(string? causationValue, string? correlationValue, out Guid causationId, out Guid correlationId)
    {
        causationId = Guid.Empty;
        correlationId = Guid.Empty;

        return causationValue is not null
            && correlationValue is not null
            && Guid.TryParse(correlationValue, out correlationId)
            && Guid.TryParse(causationValue, out causationId);
    }
}