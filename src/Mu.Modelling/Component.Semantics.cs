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

            public Characteristics Characteristics { get; set; } = Characteristics.Undefined;

            public bool HasBinder { get; set; } = true;

            public bool HasEqualsOverride { get; set; } = true;

            public bool HasGetHashCodeOverride { get; set; } = true;

            public Capabilities Identifier { get; set; } = new Capabilities();

            [Ignore]
            public bool IsOutOfScope => this == OutOfScope;

            public Capabilities Self { get; set; } = Capabilities.OutOfScope;
        }
    }
}