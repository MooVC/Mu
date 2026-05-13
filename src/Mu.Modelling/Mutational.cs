namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    public sealed partial class Mutational
        : IValidatableObject
    {
        public static readonly Mutational Undefined = new Mutational();

        internal Mutational()
        {
        }

        [Descriptor("Raises")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Fact { get; internal set; } = Name.Unnamed;

        [Descriptor("OfType")]
        [Hide]
        [Traverse(Scope = TraverseScope.None)]
        public Kinds Type { get; internal set; } = Kinds.Transitional;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .Include(nameof(Fact), _ => !Fact.IsUnnamed, Fact)
                .Results;
        }
    }
}