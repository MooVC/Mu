namespace Muify
{
    using System;
    using System.Collections.Generic;
    using Mu.Modelling;
    using Muify.Domain;
    using AreaComponent = Mu.Modelling.Model.Graph.Areas.Area.Components.Component;
    using Unit = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit;
    using UnitComponent = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Components.Component;

    public partial class ModelGenerator
    {
        private sealed class ServiceProvider
            : IServiceProvider
        {
            private static readonly Type[] _componentVisitors = new Type[]
            {
                typeof(ComponentHasEqualsOverrideVisitor),
                typeof(ComponentHasGetHashCodeOverrideVisitor),
                typeof(ComponentIdentifierHasEqualsOperatorVisitor),
                typeof(ComponentIdentifierHasEquatableVisitor),
                typeof(ComponentIdentifierHasNotEqualsOperatorVisitor),
                typeof(ComponentIdentifierIsEquatableVisitor),
                typeof(ComponentSelfHasEqualsOperatorVisitor),
                typeof(ComponentSelfHasEquatableVisitor),
                typeof(ComponentSelfHasNotEqualsOperatorVisitor),
                typeof(ComponentSelfIsEquatableVisitor),
            };

            private static readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
                { typeof(IModelVisitor<AreaComponent, File>), new CollectionVisitor<AreaComponent>(_componentVisitors) },
                { typeof(IModelVisitor<Unit, File>), new UnitBaseVisitor() },
                { typeof(IModelVisitor<UnitComponent, File>), new CollectionVisitor<UnitComponent>(_componentVisitors) },
            };

            public object GetService(Type serviceType)
            {
                if (_services.TryGetValue(serviceType, out object service))
                {
                    return service;
                }

                return default;
            }
        }
    }
}