namespace Mu.Modelling
{
    using Fluentify;
    using MooVC.Syntax.CSharp;
    using Valuify;
    using Ignore = Valuify.IgnoreAttribute;

    [Fluentify]
    [Valuify]
    internal sealed partial class Service
    {
        public static readonly Service Undefined = new Service();

        public Qualification Definition { get; set; } = Qualification.Unnamed;

        public bool HasRegistrar { get; set; } = true;

        public bool IsPartial { get; set; }

        [Ignore]
        public bool IsUndefined => this == Undefined;
    }
}