namespace Mu.Modelling;

using System;
using Mu.Modelling.State;

/// <summary>
/// Describes the aggregate model metadata used by causal messages.
/// </summary>
public sealed record Representation
{
    private static readonly Type _basis = typeof(Aggregate);

    private Representation(Type type)
    {
        if (!_basis.IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type `{type}` must derive from `{_basis}`.", nameof(type));
        }

        if (type.IsAbstract || !type.IsSealed)
        {
            throw new ArgumentException($"Type `{type}` must be a sealed, concrete derivation of `{_basis}`.", nameof(type));
        }

        Name = type.Name;
        Namespace = type.Namespace!;
        Assembly = type.Assembly.GetName().Name!;
    }

    /// <summary>
    /// Gets the assembly name containing the aggregate type.
    /// </summary>
    public string Assembly { get; }

    /// <summary>
    /// Gets the aggregate type name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the namespace containing the aggregate type.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Converts an aggregate type to its <see cref="Representation"/> representation.
    /// </summary>
    public static implicit operator Representation(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return new Representation(type);
    }

    /// <summary>
    /// Converts a <see cref="Representation"/> representation back to the runtime <see cref="Type"/>.
    /// </summary>
    public static implicit operator Type(Representation model)
    {
        ArgumentNullException.ThrowIfNull(model);

        Type type = Type.GetType($"{model.Namespace}.{model.Name}, {model.Assembly}", throwOnError: false)
            ?? throw new ArgumentException($"The type associated with `{model}` is not available to load by this process.");

        return type;
    }
}