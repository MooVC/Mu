namespace Mu.Modelling
{
    using Fluentify;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    internal sealed partial class Characteristics
    {
        public static readonly Characteristics Undefined = new Characteristics();

        public bool IsClass { get; set; }

        public bool IsInterface { get; set; }

        public bool IsRecord { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsRef { get; set; }

        public bool IsStruct { get; set; }

        [Ignore]
        public bool IsUndefined => this == Undefined;
    }
}