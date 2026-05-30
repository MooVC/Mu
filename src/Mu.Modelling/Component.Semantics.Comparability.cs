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
            internal sealed partial class Comparability
            {
                [SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "API Standard")]
                public static readonly Comparability OutOfScope = new Comparability();

                public bool HasCompareTo { get; set; }

                [Descriptor("IsComparable")]
                public Presence IsComparable { get; set; } = Presence.NotApplicable;

                [Ignore]
                public bool IsOutOfScope => this == OutOfScope;
            }
        }
    }
}