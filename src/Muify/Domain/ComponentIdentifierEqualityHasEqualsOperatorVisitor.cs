namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class ComponentIdentifierEqualityHasEqualsOperatorVisitor
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
            if (component.Metadata.Identifier.Equality.HasEqualsOperator && !component.Identifier.IsUndefined)
            {
                yield break;
            }

            var content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Named(component.Name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithOperators(operators => operators
                        .WithComparisons(equals => equals
                            .To(component.Identifier.Type)
                            .WithBody("return left is not null && left.Equals(right);")
                            .WithOperator(Comparison.Types.Equality)))
                    .WithScope(Scopes.Unspecified))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{component.Name}.Identifier.Comparison.Equals");
        }
    }
}