namespace Mu.Modelling;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Fluentify;
using Graphify;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using MooVC.Syntax.Validation;
using Valuify;
using Ignore = Valuify.IgnoreAttribute;

[Fluentify]
[Valuify]
public sealed partial class Result
    : IValidatableObject
{
    public static readonly Result Undefined = new();

    internal Result()
    {
    }

    [Descriptor("DescribedAs")]
    [Traverse(Scope = TraverseScope.None)]
    public Description Description { get; internal init; } = Description.Undescribed;

    [Ignore]
    [Traverse(Scope = TraverseScope.None)]
    public bool IsUndefined => this == Undefined;

    [Descriptor("Named")]
    [Traverse(Scope = TraverseScope.None)]
    public Name Name { get; internal init; } = Name.Unnamed;

    [Descriptor("OfType")]
    [Traverse(Scope = TraverseScope.None)]
    public Symbol Type { get; internal init; } = Symbol.Undefined;

    public static implicit operator Result((Name Name, Symbol Type) source)
    {
        Guard.Against.Conversion<(Name Name, Symbol Type), Attribute>(source);

        return new Result()
            .Named(source.Name)
            .OfType(source.Type);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsUndefined)
        {
            return [];
        }

        return validationContext
            .Include(nameof(Name), _ => !Name.IsUnnamed, Name)
            .And(nameof(Type), _ => !Type.IsUndefined, Type)
            .Results;
    }
}