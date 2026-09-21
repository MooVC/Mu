namespace Mu.Communications.Tracing;

using System;
using Ardalis.GuardClauses;
using Grpc.Core;
using Mu.Modelling.Behavior;
using ProtoBuf.Grpc;

public static partial class CallContextExtensions
{
    private const string CausationHeader = "causation";
    private const string CorrelationHeader = "correlation";

    public static Ledger ToLedger(this CallContext context, UseCase useCase)
    {
        _ = Guard.Against.Null(useCase, message: "The use case must be provided.");

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