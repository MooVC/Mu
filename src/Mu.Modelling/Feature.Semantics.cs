namespace Mu.Modelling
{
    using System.Collections.Immutable;
    using Fluentify;
    using MooVC.Syntax.CSharp;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Feature
    {
        [Fluentify]
        [Valuify]
        internal sealed partial class Semantics
        {
            public static readonly Semantics OutOfScope = new Semantics();

            public bool HasBase { get; set; } = true;

            public bool HasFact { get; set; } = true;

            public bool HasRegistrar { get; set; } = true;

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;

            public ImmutableArray<Qualification> Registrars { get; set; } = ImmutableArray<Qualification>.Empty;

            public ImmutableArray<Qualification> Transforms { get; set; } = ImmutableArray<Qualification>.Empty;
        }
    }
}