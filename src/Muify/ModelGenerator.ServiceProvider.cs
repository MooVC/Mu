namespace Muify
{
    using System;
    using System.Collections.Generic;
    using Mu.Modelling;
    using Muify.Domain;
    using Muify.Service;
    using AreaComponent = Mu.Modelling.Model.Graph.Areas.Area.Components.Component;
    using Feature = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Features.Feature;
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
                typeof(ComponentIdentifierComparabilityHasCompareToVisitor),
                typeof(ComponentIdentifierComparabilityHasGreaterThanOperatorVisitor),
                typeof(ComponentIdentifierComparabilityHasGreaterThanOrEqualOperatorVisitor),
                typeof(ComponentIdentifierComparabilityHasLessThanOperatorVisitor),
                typeof(ComponentIdentifierComparabilityHasLessThanOrEqualOperatorVisitor),
                typeof(ComponentIdentifierComparabilityIsComparableVisitor),
                typeof(ComponentIdentifierEqualityHasEqualsOperatorVisitor),
                typeof(ComponentIdentifierEqualityHasEquatableVisitor),
                typeof(ComponentIdentifierEqualityHasNotEqualsOperatorVisitor),
                typeof(ComponentIdentifierEqualityIsEquatableVisitor),
                typeof(ComponentIdentifierHasImplicitConversionVisitor),
                typeof(ComponentSelfComparabilityHasCompareToVisitor),
                typeof(ComponentSelfComparabilityHasGreaterThanOperatorVisitor),
                typeof(ComponentSelfComparabilityHasGreaterThanOrEqualOperatorVisitor),
                typeof(ComponentSelfComparabilityHasLessThanOperatorVisitor),
                typeof(ComponentSelfComparabilityHasLessThanOrEqualOperatorVisitor),
                typeof(ComponentSelfComparabilityIsComparableVisitor),
                typeof(ComponentSelfEqualityHasEqualsOperatorVisitor),
                typeof(ComponentSelfEqualityHasEquatableVisitor),
                typeof(ComponentSelfEqualityHasNotEqualsOperatorVisitor),
                typeof(ComponentSelfEqualityIsEquatableVisitor),
            };

            private static readonly Type[] _featureVisitors = new Type[]
            {
                typeof(FeatureFactVisitor),
                typeof(FeatureTransformVisitor),
            };

            private static readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
                { typeof(IModelVisitor<AreaComponent, File>), new CollectionVisitor<AreaComponent>(_componentVisitors) },
                { typeof(IModelVisitor<Feature, File>), new CollectionVisitor<Feature>(_featureVisitors) },
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