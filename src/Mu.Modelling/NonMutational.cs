namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    public sealed partial class NonMutational
        : IValidatableObject
    {
        public static readonly NonMutational Undefined = new NonMutational();

        internal NonMutational()
        {
        }

        [Descriptor("From")]
        [Hide]
        [Traverse(Scope = TraverseScope.None)]
        public Kinds Source { get; internal set; } = Kinds.ReadStore;

        [Descriptor("Using")]
        [Traverse(Scope = TraverseScope.None)]
        public View View { get; internal set; } = View.Undefined;

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
                .Include(nameof(View), _ => !View.IsUndefined, View)
                .Results;
        }
    }
}