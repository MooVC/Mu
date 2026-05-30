namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class ComponentIdentifierComparabilityHasCompareToVisitor
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
            if (component.Identifier.IsUndefined
             || component.Metadata.Identifier.Comparability.IsComparable == Presence.NotApplicable
             || component.Metadata.Identifier.Comparability.HasCompareTo)
            {
                yield break;
            }

            var content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Named(component.Name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithMethods(compareTo => compareTo
                        .Accepts(parameter => parameter
                            .Named("Other")
                            .OfType(component.Identifier.Type, type => type.IsNullable(true)))
                        .Named("CompareTo")
                        .Returns(typeof(int), result => result.WithMode(Result.Modes.Synchronous))
                        .WithBody($"return other is null ? 1 : other.CompareTo({component.Identifier.Name});"))
                    .WithScope(Scopes.Unspecified))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{component.Name}.Identifier.IComparable.CompareTo");
        }
    }
}