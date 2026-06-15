namespace Mu.Modelling.Services;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using Mu.Persistence;

/// <summary>
/// Executes transitional use cases by loading a target aggregate, applying the mutation, and persisting resulting facts.
/// </summary>
public sealed class TransitionalService<TAggregate, TIdentity, TUseCase>(IRoot<TAggregate, TUseCase> root, IWriteStore<TAggregate, TIdentity> store)
    : IService<TUseCase, Revision>
    where TAggregate : Aggregate, new()
    where TIdentity : struct
    where TUseCase : Transitional<TAggregate, TIdentity>
{
    /// <summary>
    /// Executes the transitional use case.
    /// </summary>
    public async Task<Result<Revision>> Execute(TUseCase useCase, CancellationToken cancellationToken)
    {
        TAggregate? aggregate = await store
            .Get(useCase.Target.Identity, useCase.Target.Revision, cancellationToken)
            .ConfigureAwait(false);

        if (aggregate is null)
        {
            return new ValidationResult($"`{typeof(TAggregate)}` `{useCase.Target}` does not exist.");
        }

        return await root
            .Apply(aggregate, useCase, cancellationToken)
            .Then(opened => store.Save(aggregate, useCase.Target.Identity, cancellationToken))
            .Select(_ => aggregate.Revision)
            .ConfigureAwait(false);
    }
}