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

    [Fluentify]
    [Valuify]
    public sealed partial class View
        : IValidatableObject
    {
        public static readonly View Undefined = new View();

        internal View()
        {
        }

        [Descriptor("AttributedWith")]
        [Traverse(Scope = TraverseScope.None)]
        public ImmutableArray<Attribute> Attributes { get; internal set; } = ImmutableArray<Attribute>.Empty;

        [Descriptor("DescribedAs")]
        [Traverse(Scope = TraverseScope.None)]
        public Description Description { get; internal set; } = Description.Undescribed;

        [Descriptor("RenderedOn")]
        public ImmutableArray<Qualifier> Facts { get; internal set; } = ImmutableArray<Qualifier>.Empty;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsReference => !Name.IsUnnamed
            && Attributes.IsDefaultOrEmpty
            && Description.IsUndescribed
            && Facts.IsDefaultOrEmpty;

        [Ignore]
        [Traverse(Scope = TraverseScope.None)]
        public bool IsUndefined => this == Undefined;

        [Descriptor("Named")]
        [Traverse(Scope = TraverseScope.None)]
        public Name Name { get; internal set; } = Name.Unnamed;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsUndefined)
            {
                return new ValidationResult[0];
            }

            return validationContext
                .IncludeIf(!Attributes.IsDefaultOrEmpty, nameof(Attributes), attribute => !attribute.IsUndefined, Attributes)
                .AndIf(!Facts.IsDefaultOrEmpty, nameof(Facts), fact => !fact.IsUnqualified, Facts)
                .And(nameof(Name), name => !name.IsUnnamed, Name)
                .Results;
        }
    }
}