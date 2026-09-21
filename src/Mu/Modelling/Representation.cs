namespace Mu.Modelling;

using System;
using MooVC;
using Mu.Modelling.State;
using ProtoBuf;
using static Mu.Modelling.Representation_Resources;

/// <summary>
/// Describes the aggregate model metadata used by causal messages.
/// </summary>
[ProtoContract(SkipConstructor = true)]
public sealed record Representation
{
    private static readonly Type _basis = typeof(Aggregate);

    private Representation(Type type)
    {
        if (!_basis.IsAssignableFrom(type))
        {
            throw new ArgumentException(TypeAggregateRequired.Format(type, _basis), nameof(type));
        }

        if (type != _basis && (type.IsAbstract || !type.IsSealed))
        {
            throw new ArgumentException(TypeSealedConcreteRequired.Format(type, _basis), nameof(type));
        }

        if (type.Assembly is null || type.FullName is null || type.Namespace is null)
        {
            throw new ArgumentException(TypeFullNameRequired.Format(type), nameof(type));
        }

        Name = type.FullName[(type.Namespace.Length + 1)..];
        Namespace = type.Namespace;
        Assembly = type.Assembly.GetName().Name ?? string.Empty;
    }

    /// <summary>
    /// Gets the assembly name containing the aggregate type.
    /// </summary>
    [ProtoMember(1, Name = nameof(Assembly))]
    public string Assembly { get; }

    /// <summary>
    /// Gets the aggregate type name.
    /// </summary>
    [ProtoMember(2, Name = nameof(Name))]
    public string Name { get; }

    /// <summary>
    /// Gets the namespace containing the aggregate type.
    /// </summary>
    [ProtoMember(3, Name = nameof(Namespace))]
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
            ?? throw new ArgumentException(ImplicitModelTypeRequired.Format(model));

        return type;
    }
}