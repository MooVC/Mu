namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using Ardalis.GuardClauses;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Valuify]
    [Fluentify]
    public sealed partial class Member
        : IValidatableObject
    {
        public static readonly Member Undefined = new Member();

        internal Member()
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

        public static implicit operator Member(Name name)
        {
            Guard.Against.Conversion<Name, Member>(name);

            return new Member()
                .Named(name);
        }

        public static implicit operator Member((Description Description, Name Name) member)
        {
            Guard.Against.Conversion<(Description Description, Name Name), Member>(member);

            return new Member()
                .DescribedAs(member.Description)
                .Named(member.Name);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .Include(nameof(Name), name => !name.IsUnnamed, Name)
                .Results;
        }
    }
}