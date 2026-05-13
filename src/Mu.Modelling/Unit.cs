namespace Mu.Modelling
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using Fluentify;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using MooVC.Syntax.Validation;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    public sealed partial class Unit
        : IValidatableObject
    {
        public static readonly Unit Undefined = new Unit();

        internal Unit()
        {
        }

        [Descriptor("AttributedWith")]
        [Traverse(Scope = TraverseScope.None)]
        public ImmutableArray<Attribute> Attributes { get; internal set; } = ImmutableArray<Attribute>.Empty;

        [Descriptor("Owns")]
        public ImmutableArray<Component> Components { get; internal set; } = ImmutableArray<Component>.Empty;

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Descriptor("Featuring")]
        public ImmutableArray<Feature> Features { get; internal set; } = ImmutableArray<Feature>.Empty;

        [Descriptor("IdentifiedBy")]
        public Qualification Identity { get; internal set; } = typeof(Guid);

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Sets")]
        public ImmutableArray<List> Lists { get; internal set; } = ImmutableArray<List>.Empty;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        [Descriptor("SeenAs")]
        public ImmutableArray<View> Views { get; internal set; } = ImmutableArray<View>.Empty;

        internal Semantics Metadata { get; set; } = Semantics.OutOfScope;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return Array.Empty<ValidationResult>();
            }

            return validationContext
                .IncludeIf(!Attributes.IsDefaultOrEmpty, nameof(Attributes), attribute => !attribute.IsUndefined, Attributes)
                .AndIf(!Components.IsDefaultOrEmpty, nameof(Components), component => !component.IsUndefined, Components)
                .AndIf(!Features.IsDefaultOrEmpty, nameof(Features), feature => !feature.IsUndefined, Features)
                .AndIf(!Lists.IsDefaultOrEmpty, nameof(Lists), list => !list.IsUndefined, Lists)
                .And(nameof(Identity), identity => !identity.IsUnnamed, Identity)
                .And(nameof(Name), name => !name.IsUnnamed, Name)
                .AndIf(!Views.IsDefaultOrEmpty, nameof(Views), view => !view.IsUndefined, Views)
                .Results;
        }
    }
}