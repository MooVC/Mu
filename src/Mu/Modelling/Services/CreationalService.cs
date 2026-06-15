namespace Mu.Modelling.Services;

using System.Linq;
using System.Threading.Tasks;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using Mu.Persistence;

/// <summary>
/// Executes creational use cases by allocating an identity, applying the mutation, and persisting <see langword="new"/> facts.
/// </summary>
public sealed class CreationalService<TAggregate, TIdentity, TUseCase>(IAllocator<TIdentity> allocator, IRoot<TAggregate, TUseCase> root, IWriteStore<TAggregate, TIdentity> store)
    : IService<TUseCase, TIdentity>
    where TAggregate : Aggregate, new()
    where TIdentity : struct
    where TUseCase : Creational
{
    /// <summary>
    /// Executes the creational use case.
    /// </summary>
    public async Task<Result<TIdentity>> Execute(TUseCase useCase, CancellationToken cancellationToken)
    {
        TIdentity identity = await allocator
            .Allocate(useCase, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var aggregate = new TAggregate();

            return await root
                .Apply(aggregate, useCase, cancellationToken)
                .Then(opened => store.Save(aggregate, identity, cancellationToken))
                .Select(_ => identity)
                .ConfigureAwait(false);
        }
        catch
        {
            await allocator
                .Surrender(identity, cancellationToken)
                .ConfigureAwait(false);

            throw;
        }
    }
}