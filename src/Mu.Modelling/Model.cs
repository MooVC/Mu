namespace Mu.Modelling;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Fluentify;
using Graphify;
using MooVC;
using MooVC.Syntax;
using MooVC.Syntax.Validation;
using Valuify;
using static Mu.Modelling.Model_Resources;
using Ignore = Valuify.IgnoreAttribute;

[Fluentify]
[Graphify]
[Valuify]
public sealed partial class Model
    : IValidatableObject
{
    public static readonly Model Undefined = new();

    [Descriptor("Defines")]
    public ImmutableArray<Area> Areas { get; internal init; } = [];

    [Descriptor("For")]
    [Traverse(Scope = TraverseScope.None)]
    public Name Company { get; internal init; } = Name.Unnamed;

    [Descriptor("DescribedAs")]
    [Traverse(Scope = TraverseScope.None)]
    public Description Description { get; internal init; } = Description.Undescribed;

    [Ignore]
    [Traverse(Scope = TraverseScope.None)]
    public bool IsUndefined => this == Undefined;

    [Descriptor("Named")]
    [Traverse(Scope = TraverseScope.None)]
    public Name Name { get; internal init; } = Name.Unnamed;

    [Traverse(Scope = TraverseScope.None)]
    public Options Options { get; internal init; } = Options.Default;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsUndefined)
        {
            return [];
        }

        IEnumerable<ValidationResult> results = Enumerable.Empty<ValidationResult>();

        if (Areas.IsDefaultOrEmpty)
        {
            results = [new ValidationResult(ValidateAreasRequired.Format(nameof(Model), nameof(Area)), [nameof(Areas)])];
        }

        return validationContext
            .IncludeIf(!Areas.IsDefaultOrEmpty, nameof(Areas), area => !area.IsUndefined, results, Areas)
            .AndIf(!Company.IsUnnamed, nameof(Company), Company)
            .And(nameof(Name), name => !name.IsUnnamed, Name)
            .Results;
    }
}