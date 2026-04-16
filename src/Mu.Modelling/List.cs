namespace Mu.Modelling
{
    using System.Collections.Generic;

    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Valuify]
    [Fluentify]
    public sealed partial class List
        : IValidatableObject
    {
        public static readonly List Undefined = new List();

        internal List()
        {
        }

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Descriptor("Containing")]
        [Traverse(Scope = TraverseScope.None)]
        public ImmutableArray<Member> Members { get; internal set; } = ImmutableArray<Member>.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return new ValidationResult[0];
            }

            return validationContext
                .Include(nameof(Name), name => !name.IsUnnamed, Name)
                .AndIf(!Members.IsDefaultOrEmpty, nameof(Members), member => !member.IsUndefined, Members)
                .Results;
        }
    }
}