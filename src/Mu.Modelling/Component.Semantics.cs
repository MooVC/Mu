namespace Mu.Modelling
{
    using Fluentify;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Component
    {
        [Fluentify]
        [Valuify]
        internal sealed partial class Semantics
        {
            public static readonly Semantics OutOfScope = new Semantics();

            public Equality Identifier { get; set; } = Equality.OutOfScope;

            public bool HasGetHashCodeOverride { get; set; } = true;

            public bool HasNotEqualsOperator { get; set; } = true;

            public Equality Self { get; set; } = Equality.OutOfScope;

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;
        }
    }
}