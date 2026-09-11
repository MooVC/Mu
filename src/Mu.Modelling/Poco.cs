namespace Mu.Modelling
{
    using System.Collections.Immutable;
    using Fluentify;
    using MooVC.Syntax.CSharp;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    internal sealed partial class Poco
    {
        public static readonly Poco Undefined = new Poco();

        public ImmutableArray<Attribute> Attributes { get; set; } = ImmutableArray<Attribute>.Empty;

        public Characteristics Characteristics { get; set; } = Characteristics.Undefined;

        public bool IsPartial { get; set; }

        [Ignore]
        public bool IsUndefined => this == Undefined;

        public Qualification Qualification { get; set; } = Qualification.Unnamed;
    }
}