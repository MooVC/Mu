namespace Mu.Modelling.Services;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Mu.Modelling.Behavior;
using Mu.Modelling.Integrity;
using Mu.Modelling.State;

public sealed class Root<TAggregate, TFact, TMutation>(IEnumerable<IInvariant<TAggregate, TMutation>> invariants, IEnumerable<ITransform<TAggregate, TFact>> transforms)
    : IRoot<TAggregate, TMutation>
    where TAggregate : Aggregate
    where TFact : Fact<TAggregate>, IConvertFrom<TFact, TMutation>
    where TMutation : Mutational
{
    public async Task<Result<TAggregate>> Apply(TAggregate aggregate, TMutation mutation, CancellationToken cancellationToken)
    {
        ImmutableArray<ValidationResult> failures = await invariants
            .EnforceAll(aggregate, mutation, cancellationToken)
            .ConfigureAwait(false);

        if (failures.Length == 0)
        {
            return aggregate.Propose(mutation, transforms);
        }

        return failures;
    }
}