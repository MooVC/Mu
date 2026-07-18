namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Fluentify;
    using Graphify;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.Validation;
    using Valuify;
    using static Mu.Modelling.Model_Resources;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
#if NET5_0_OR_GREATER
    [Graphify]
#else
    [Graphify(Mode = Modes.Synchronous)]
#endif
    [Valuify]
    public sealed partial class Model
        : IValidatableObject
    {
        public static readonly Model Undefined = new Model();

        [Descriptor("Defines")]
        public ImmutableArray<Area> Areas { get; internal set; } = ImmutableArray<Area>.Empty;

        [Descriptor("For")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Company { get; internal set; } = Name.Unnamed;

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Traverse(Scope = TraverseScope.Property)]
        public Options Options { get; internal set; } = Options.Default;

        internal Semantics Metadata { get; set; } = Semantics.OutOfScope;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            IEnumerable<ValidationResult> results = Enumerable.Empty<ValidationResult>();

            if (Areas.IsDefaultOrEmpty)
            {
                results = new[] { new ValidationResult(ValidateAreasRequired.Format(nameof(Model), nameof(Area)), new[] { nameof(Areas) }) };
            }

            return validationContext
                .IncludeIf(!Areas.IsDefaultOrEmpty, nameof(Areas), area => !area.IsUndefined, results, Areas)
                .AndIf(!Company.IsUnnamed, nameof(Company), Company)
                .And(nameof(Name), name => !name.IsUnnamed, Name)
                .Results;
        }
    }
}