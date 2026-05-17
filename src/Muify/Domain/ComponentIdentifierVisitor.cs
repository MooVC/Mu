namespace Muify.Domain
{
    using System.Collections.Generic;
    using Mu.Modelling;

    internal sealed class ComponentIdentifierVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Components.Component, File>,
          IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Components.Component, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Components.Component component)
        {
            return Generate(component.Value);
        }

        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Components.Component component)
        {
            return Generate(component.Value);
        }

        private static IEnumerable<File> Generate(Component component)
        {
            if (component.Identifier.IsUndefined)
            {
                yield break;
            }
        }
    }
}