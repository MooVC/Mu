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
            internal sealed partial class Equality
            {
                [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "API Standard")]
                public static readonly Equality OutOfScope = new Equality();

                public bool HasEquatable { get; set; } = true;

                public bool HasEqualsOperator { get; set; } = true;

                public bool HasEqualsOverride { get; set; } = true;

                public bool IsEquatable { get; set; } = true;

                [Ignore]
                public bool IsOutOfScope => this == OutOfScope;
            }
        }
    }
}