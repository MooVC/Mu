namespace Mu.Services;

using System.Threading.Tasks;
using Mu.Modelling.Behavior;

/// <summary>
/// Allocates identities for newly created aggregates.
/// </summary>
/// <typeparam name="TIdentity">The type used as the aggregate identity.</typeparam>
public interface IAllocator<TIdentity>
    where TIdentity : struct
{
    /// <summary>
    /// Allocates an identity for the provided use case.
    /// </summary>
    /// <typeparam name="TUseCase">The concrete use case type requesting an identity.</typeparam>
    /// <param name="useCase">The use case that triggers identity allocation.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>The allocated identity.</returns>
    ValueTask<TIdentity> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
        where TUseCase : UseCase;
}