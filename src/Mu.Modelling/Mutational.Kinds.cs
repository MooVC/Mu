namespace Mu.Modelling;

using Monify;

public partial class Mutational
{
    [Monify(Type = typeof(string))]
    public sealed partial class Kinds
    {
        public static readonly Kinds Creational = "Creational";
        public static readonly Kinds Transitional = "Transitional";

        private Kinds(string value)
        {
            _value = value;
        }

        public bool IsCreational => this == Creational;

        public bool IsTransitional => this == Transitional;

        public override string ToString()
        {
            return _value;
        }
    }
}