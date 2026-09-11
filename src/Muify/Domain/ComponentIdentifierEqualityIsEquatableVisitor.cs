namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class ComponentIdentifierEqualityIsEquatableVisitor
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
            if (component.Metadata.Identifier.Equality.IsEquatable || component.Identifier.IsUndefined)
            {
                yield break;
            }

            var identifer = component.Identifier.Type.ToSnippet(Configuration.Options.Types);

            var content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .DerivesFrom((Name: $"IEquatable<{identifer}>", Qualifier: "System"))
                    .Named(component.Name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithScope(Scopes.Unspecified))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{component.Name}.Identifier.IEquatable");
        }
    }
}