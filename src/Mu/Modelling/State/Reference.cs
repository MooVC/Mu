namespace Mu.Modelling.State;

/// <summary>
/// Represents a reference to a specific aggregate identity and revision.
/// </summary>
/// <typeparam name="TIdentity">The type used as aggregate identity.</typeparam>
public readonly record struct Reference<TIdentity>
    where TIdentity : struct
{
    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="Reference{TIdentity}"/> record.
    /// </summary>
    internal Reference(TIdentity identity, ulong revision)
    {
        Identity = identity;
        Revision = revision;
    }

    /// <summary>
    /// Gets a value indicating whether the reference points to an unspecified revision.
    /// </summary>
    public bool IsUnspecified => Revision == ulong.MinValue;

    /// <summary>
    /// Gets the aggregate identity.
    /// </summary>
    public TIdentity Identity { get; }

    /// <summary>
    /// Gets the aggregate revision.
    /// </summary>
    public ulong Revision { get; }

    /// <summary>
    /// Defines an implicit conversion from a <see cref="Reference{TIdentity}"/> to its underlying identity type.
    /// </summary>
    /// <param name="reference">The reference to convert.</param>
    /// <returns>The underlying identity.</returns>
    public static implicit operator TIdentity(Reference<TIdentity> reference)
    {
        return reference.Identity;
    }
}