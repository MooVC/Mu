namespace Mu.Modelling
{
    using Monify;

    public partial class NonMutational
    {
        [Monify(Type = typeof(string))]
        public sealed partial class Kinds
        {
            public static readonly Kinds ReadStore = "ReadStore";
            public static readonly Kinds WriteStore = "WriteStore";

            private Kinds(string value)
            {
                _value = value;
            }

            public bool IsReadStore => this == ReadStore;

            public bool IsWriteStore => this == WriteStore;

            public override string ToString()
            {
                return _value;
            }
        }
    }
}