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
    public sealed partial class Area
        : IValidatableObject
    {
        public static readonly Area Undefined = new Area();

        internal Area()
        {
        }

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Descriptor("Owns")]
        public ImmutableArray<Component> Components { get; internal set; } = ImmutableArray<Component>.Empty;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Sets")]
        public ImmutableArray<List> Lists { get; internal set; } = ImmutableArray<List>.Empty;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Descriptor("ResponsibleFor")]
        public ImmutableArray<Unit> Units { get; internal set; } = ImmutableArray<Unit>.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .IncludeIf(!Components.IsDefaultOrEmpty, nameof(Components), component => !component.IsUndefined, Components)
                .AndIf(!Lists.IsDefaultOrEmpty, nameof(Lists), list => !list.IsUndefined, Lists)
                .And(nameof(Name), name => !name.IsUnnamed, Name)
                .AndIf(!Units.IsDefaultOrEmpty, nameof(Units), unit => !unit.IsUndefined, Units)
                .Results;
        }
    }
}