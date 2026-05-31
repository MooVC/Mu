namespace Muify.Domain
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Domain.ComponentIdentifierHasImplicitConversionVisitor_Resources;
    using Conversion = MooVC.Syntax.CSharp.Conversion;

    internal sealed class ComponentIdentifierHasImplicitConversionVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Components.Component, File>,
          IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Components.Component, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Components.Component component)
        {
            return Generate(component.Value, component.Namespace);
        }

        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Components.Component component)
        {
            return Generate(component.Value, component.Namespace);
        }

        private static IEnumerable<File> Generate(Component component, Qualifier @namespace)
        {
            if (component.Identifier.IsUndefined || component.Metadata.Identifier.HasImplicitConversion)
            {
                yield break;
            }

            string body = Body.Format(component.Identifier.Name);

            var content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Named(component.Name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithOperators(operators => operators
                        .WithConversions(conversion => conversion
                            .ForType(component.Identifier.Type)
                            .WithBody(Snippet.From(body))
                            .WithDirection(Conversion.Intents.To)
                            .WithMode(Conversion.Types.Implicit)))
                    .WithScope(Scopes.Unspecified))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{component.Name}.Identifier.Conversion.Implicit");
        }
    }
}