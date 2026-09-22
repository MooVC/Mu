namespace Mu.Communications.Tracing;

using Mu.Modelling.Behavior;

/// <summary>
/// Manages the ambient trace ledger for nested use case execution.
/// </summary>
public interface IScribe
{
    /// <summary>
    /// Enters a use case scope using the active parent's identity as causation and preserving correlation.
    /// </summary>
    /// <param name="useCase">The use case entering the scope.</param>
    /// <param name="current">The current ledger at the time of entering the scope.</param>
    /// <returns>A lease that restores the previous ledger and use case identity when disposed.</returns>
    /// <remarks>
    /// Without an active scope, the use case identity becomes both causation and correlation.
    /// The first use case within a scope established by <see cref="Set"/> uses the supplied ledger unchanged.
    /// Dispose leases in reverse order of creation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The use case is <see langword="null"/>.</exception>
    IDisposable Next(UseCase useCase, out Ledger current);

    /// <summary>
    /// Enters a root scope with an incoming ledger when no scope is active.
    /// </summary>
    /// <param name="ledger">The ledger from an existing message chain.</param>
    /// <returns>A lease that clears the ledger when disposed after its nested scopes.</returns>
    /// <exception cref="InvalidOperationException">A ledger scope is already active.</exception>
    IDisposable Set(Ledger ledger);
}