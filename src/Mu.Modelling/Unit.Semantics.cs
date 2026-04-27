namespace Mu.Modelling
{
    using Fluentify;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Unit
    {
        [Valuify]
        internal sealed partial class Semantics
        {
            public static readonly Semantics OutOfScope = new Semantics();

            public bool HasBase { get; set; } = true;

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;
        }
    }
}