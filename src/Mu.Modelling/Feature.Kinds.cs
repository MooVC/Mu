namespace Mu.Modelling
{
    using Monify;

    public partial class Feature
    {
        [Monify(Type = typeof(string))]
        public sealed partial class Kinds
        {
            public static readonly Kinds Mutational = "Mutational";
            public static readonly Kinds NonMutational = "NonMutational";

            private Kinds(string value)
            {
                _value = value;
            }

            public bool IsMutational => this == Mutational;

            public bool IsNonMutational => this == NonMutational;

            public override string ToString()
            {
                return _value;
            }
        }
    }
}