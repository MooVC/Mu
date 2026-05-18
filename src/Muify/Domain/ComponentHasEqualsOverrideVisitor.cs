namespace Muify.Domain
{
    using System;
    using System.Collections.Generic;
    using MooVC.Syntax;
    using Mu.Modelling;

    internal sealed class ComponentHasEqualsOverrideVisitor
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

        private static IEnumerable<File> Generate(Component value, Qualifier @namespace)
        {
            throw new NotImplementedException();
        }
    }
}