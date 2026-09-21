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

            if (causationValue is null
             || correlationValue is null
             || !Guid.TryParse(correlationValue, out Guid correlationId)
             || !Guid.TryParse(causationValue, out Guid causationId))
            {
                return new Ledger(useCase.Identity);
            }

            return new Ledger(causationId, correlationId);
        }
    }
}