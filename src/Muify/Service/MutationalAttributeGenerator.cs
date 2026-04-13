namespace Muify.Service
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Attribute = System.Attribute;

    public abstract class MutationalAttributeGenerator
        : IIncrementalGenerator
    {
        private readonly string _hint;
        private readonly string _name;

        private protected MutationalAttributeGenerator(string hint, string name)
        {
            _hint = hint;
            _name = name;
        }

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(Generate);
        }

        private void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            var content = Builder
                .New<Definition>()
                .From(typeof(CreationalAttributeGenerator))
                .For<Class>(@class => @class
                    .AttributedWith(attribute => attribute
                        .Named(typeof(AttributeUsageAttribute))
                        .WithArguments(
                            (Name: string.Empty, Value: "global::System.AttributeTargets.Class"),
                            (Name: nameof(AttributeUsageAttribute.AllowMultiple), Value: "false"),
                            (Name: nameof(AttributeUsageAttribute.Inherited), Value: "false")))
                    .AttributedWith(attribute => attribute
                        .Named((Name: "EmbeddedAttribute", Qualifier: "Microsoft.CodeAnalysis")))
                    .DerivesFrom(typeof(Attribute))
                    .Named($"{_name}Attribute")
                    .WithProperties(name => name
                        .Named("Fact")
                        .OfType(typeof(string))
                        .WithBehaviours(behaviors => behaviors
                            .WithSet(set => set.WithMode(Property.Methods.Setter.Modes.Set))))
                    .WithScope(Scopes.Internal))
                .ToSnippet(Configuration.Options);

            context.AddSource(_hint, SourceText.From(content, Encoding.UTF8));
        }
    }
}