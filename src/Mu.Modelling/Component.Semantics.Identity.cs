namespace Mu.Modelling
{
    using System.Diagnostics.CodeAnalysis;
    using Fluentify;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    public partial class Component
    {
        internal partial class Semantics
        {
            [Fluentify]
            [Valuify]
            internal sealed partial class Identity
            {
                [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "API Standard")]
                public static readonly Identity OutOfScope = new Identity();

                public Comparability Comparability { get; set; } = Comparability.OutOfScope;

                public Equality Equality { get; set; } = Equality.OutOfScope;

                public bool HasImplicitConversion { get; set; } = true;

                [Ignore]
                public bool IsOutOfScope => this == OutOfScope;
            }
        }
    }
}