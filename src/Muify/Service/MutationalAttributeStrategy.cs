namespace Muify.Service
{
    using System;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Muify.Syntax.CSharp;
    using Attribute = System.Attribute;

    internal abstract class MutationalAttributeStrategy
        : AttributeStrategy
    {
        private readonly string _name;

        private protected MutationalAttributeStrategy(string hint, string name)
            : base(hint)
        {
            _name = name;
        }

        protected override Snippet GetContent()
        {
            Symbol targets = typeof(AttributeTargets);

            return Builder
                .New<Definition>()
                .From(typeof(CreationalAttributeStrategy))
                .For<Class>(@class => @class
                    .AddEmbeddedAttribute()
                    .AttributedWith(usage => usage
                        .Named(typeof(AttributeUsageAttribute))
                        .WithArguments(
                            (Name: string.Empty, Value: $"{targets.ToSnippet(Configuration.Options)}.Class"),
                            (Name: nameof(AttributeUsageAttribute.AllowMultiple), Value: "false"),
                            (Name: nameof(AttributeUsageAttribute.Inherited), Value: "false")))
                    .DerivesFrom(typeof(Attribute))
                    .Named(
                        $"{_name}Attribute",
                        attribute => attribute
                            .WithArguments(fact => fact
                                .Named("TFact")
                                .WithConstraints(constraint => constraint
                                    .WithBase((Name: "Fact", Qualifier: "Mu.Modelling.Behavior"))))))
                .ToSnippet(Configuration.Options);
        }
    }
}