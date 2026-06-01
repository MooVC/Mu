namespace Muify.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class ComponentSelfEqualityHasEquatableVisitor
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
            if (component.Identifier.IsUndefined || component.Metadata.Self.Equality.HasEquatable)
            {
                yield break;
            }

            var content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Named(component.Name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithMethods(equals => equals
                        .Accepts(parameter => parameter
                            .Named("Other")
                            .OfType((component.Name, Qualifier: @namespace), type => type.IsNullable(true)))
                        .Named("Equals")
                        .Returns(typeof(bool), result => result.WithMode(Result.Modes.Synchronous))
                        .WithBody($"return Equals(other.{component.Identifier.Name});"))
                    .WithScope(Scopes.Unspecified))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{component.Name}.Self.IEquatable.Equals");
        }
    }
}