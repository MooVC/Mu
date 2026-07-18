namespace Mu.Modelling
{
    using System.Collections.Immutable;
    using Fluentify;
    using Graphify;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Model
    {
        [Fluentify]
        [Valuify]
        internal sealed partial class Semantics
        {
            public static readonly Semantics OutOfScope = new Semantics();

            [Traverse(Scope = TraverseScope.None)]
            public ImmutableArray<string> Assemblies { get; set; } = ImmutableArray<string>.Empty;

            public bool HasComposition { get; set; }

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;
        }
    }
}