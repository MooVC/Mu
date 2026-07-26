namespace Mu.Sample.Open;

using Mu.Communications.Mediation;

public sealed class OpenGrpcService(IMediator mediator)
    : IOpenFeature
{
    public async Task<OpenResponse> Execute(Open request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<Guid> result = await mediator
            .Execute<Open, Guid>(request, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccessful)
        {
            return new()
            {
                AccountId = result.Value.ToString(),
                Successful = true,
            };
        }

        return new()
        {
            Failures = [.. result.Failures.Select(failure => failure.ErrorMessage ?? string.Empty)],
        };
    }
}