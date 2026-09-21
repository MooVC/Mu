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
    using UnitIdentity = Mu.Modelling.Model.Graph.Areas.Area.Units.Unit.Identity;

    public partial class ModelGenerator
    {
        private sealed class ServiceProvider
            : IServiceProvider
        {
            private static readonly Type[] _componentVisitors = new Type[]
            {
                typeof(GenerateEqualsOverrideWhenComponentVisited),
                typeof(GenerateGetHashCodeOverrideWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityCompareToWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityGreaterThanOperatorWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityGreaterThanOrEqualOperatorWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityLessThanOperatorWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityLessThanOrEqualOperatorWhenComponentVisited),
                typeof(GenerateIdentifierComparabilityComparableInterfaceWhenComponentVisited),
                typeof(GenerateIdentifierEqualityEqualsOperatorWhenComponentVisited),
                typeof(GenerateIdentifierEqualityEqualsWhenComponentVisited),
                typeof(GenerateIdentifierEqualityNotEqualsOperatorWhenComponentVisited),
                typeof(GenerateIdentifierEqualityEquatableInterfaceWhenComponentVisited),
                typeof(GenerateIdentifierImplicitConversionWhenComponentVisited),
                typeof(GenerateSelfComparabilityCompareToWhenComponentVisited),
                typeof(GenerateSelfComparabilityGreaterThanOperatorWhenComponentVisited),
                typeof(GenerateSelfComparabilityGreaterThanOrEqualOperatorWhenComponentVisited),
                typeof(GenerateSelfComparabilityLessThanOperatorWhenComponentVisited),
                typeof(GenerateSelfComparabilityLessThanOrEqualOperatorWhenComponentVisited),
                typeof(GenerateSelfComparabilityComparableInterfaceWhenComponentVisited),
                typeof(GenerateSelfEqualityEqualsOperatorWhenComponentVisited),
                typeof(GenerateSelfEqualityEqualsWhenComponentVisited),
                typeof(GenerateSelfEqualityNotEqualsOperatorWhenComponentVisited),
                typeof(GenerateSelfEqualityEquatableInterfaceWhenComponentVisited),
            };

            private static readonly Type[] _featureVisitors = new Type[]
            {
                typeof(GenerateBaseWhenFeatureVisited),
                typeof(GenerateConstructorsWhenFeatureVisited),
                typeof(GenerateFactWhenFeatureVisited),
                typeof(GenerateRegistrarWhenFeatureVisited),
                typeof(GenerateTransformWhenFeatureVisited),
            };

            private static readonly Type[] _unitIdentityVisitors = new Type[]
            {
                typeof(GenerateRegistrarWhenUnitIdentityVisited),
            };

            private static readonly Type[] _unitVisitors = new Type[]
            {
                typeof(GenerateBaseWhenUnitVisited),
                typeof(GenerateBinderWhenUnitVisited),
                typeof(GenerateRegistrarWhenUnitVisited),
            };

            private static readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
                { typeof(IEnumerable<IModelVisitor<AreaComponent, File>>), new[] { new CollectionVisitor<AreaComponent>(_componentVisitors) } },
                { typeof(IEnumerable<IModelVisitor<Feature, File>>), new[] { new CollectionVisitor<Feature>(_featureVisitors) } },
                { typeof(IEnumerable<IModelVisitor<Unit, File>>), new[] { new CollectionVisitor<Unit>(_unitVisitors) } },
                { typeof(IEnumerable<IModelVisitor<UnitComponent, File>>), new[] { new CollectionVisitor<UnitComponent>(_componentVisitors) } },
                { typeof(IEnumerable<IModelVisitor<UnitIdentity, File>>), new[] { new CollectionVisitor<UnitIdentity>(_unitIdentityVisitors) } },
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