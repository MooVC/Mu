namespace Muify.Service
{
    using System;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Muify.Syntax;
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
            return Builder
                .New<Definition>()
                .From(typeof(CreationalAttributeStrategy))
                .For<Class>(@class => @class
                    .AddEmbeddedAttribute()
                    .AttributedWith(usage => usage
                        .Named(typeof(AttributeUsageAttribute))
                        .WithArguments(
                            (Name: string.Empty, Value: "global::System.AttributeTargets.Class"),
                            (Name: nameof(AttributeUsageAttribute.AllowMultiple), Value: "false"),
                            (Name: nameof(AttributeUsageAttribute.Inherited), Value: "false")))
                    .DerivesFrom(typeof(Attribute))
                    .Named($"{_name}Attribute")
                    .WithProperties(name => name
                        .Named("Fact")
                        .OfType(typeof(string))
                        .WithBehaviours(behaviors => behaviors
                            .WithSet(set => set.WithMode(Property.Methods.Setter.Modes.Set)))))
                .ToSnippet(Configuration.Options);
        }
    }
}