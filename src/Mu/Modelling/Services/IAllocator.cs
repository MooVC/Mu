namespace Mu.Modelling.Services;

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
    /// Allocates an identity for the provided use case for a temporary period until either confirmed or surrendered.
    /// </summary>
    /// <typeparam name="TUseCase">The concrete use case type requesting an identity.</typeparam>
    /// <param name="useCase">The use case that triggers identity allocation.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>The allocated identity.</returns>
    ValueTask<TIdentity> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
        where TUseCase : UseCase;

    /// <summary>
    /// Confirms a previously allocated identity, making its allocation permanent.
    /// </summary>
    /// <param name="identity">The identity to confirm.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask Confirm(TIdentity identity, CancellationToken cancellationToken);

    /// <summary>
    /// Surrenders an identity that was previously allocated for a specific use case.
    /// </summary>
    /// <param name="identity">The identity to surrender.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask Surrender(TIdentity identity, CancellationToken cancellationToken);
}