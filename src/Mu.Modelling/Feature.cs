namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    public sealed partial class Feature
        : IValidatableObject
    {
        public static readonly Feature Undefined = new Feature();

        internal Feature()
        {
        }

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Hide]
        [Traverse(Scope = TraverseScope.None)]
        public Mutational Mutational { get; internal set; } = Mutational.Undefined;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Hide]
        [Traverse(Scope = TraverseScope.None)]
        public NonMutational NonMutational { get; internal set; } = NonMutational.Undefined;

        [Descriptor("Using")]
        [Traverse(Scope = TraverseScope.None)]
        public ImmutableArray<Parameter> Parameters { get; internal set; } = ImmutableArray<Parameter>.Empty;

        [Descriptor("Returning")]
        [Traverse(Scope = TraverseScope.None)]
        public ImmutableArray<Result> Results { get; internal set; } = ImmutableArray<Result>.Empty;

        [Descriptor("OfType")]
        [Traverse(Scope = TraverseScope.None)]
        public Kinds Type { get; internal set; } = Kinds.Mutational;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .IncludeIf(Type.IsMutational, nameof(Mutational), mutational => !mutational.IsUndefined, Mutational)
                .And(nameof(Name), _ => !Name.IsUnnamed, Name)
                .AndIf(Type.IsNonMutational, nameof(NonMutational), nonmutational => !nonmutational.IsUndefined, NonMutational)
                .AndIf(!Parameters.IsDefaultOrEmpty, nameof(Parameters), parameter => !parameter.IsUndefined, Parameters)
                .Results;
        }
    }
}