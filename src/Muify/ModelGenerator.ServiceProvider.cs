namespace Muify
{
    using System;
    using System.Collections.Generic;
    using Mu.Modelling;
    using Muify.Domain;

    public partial class ModelGenerator
    {
        private sealed class ServiceProvider
            : IServiceProvider
        {
            private static readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
                { typeof(IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>), new UnitBaseVisitor() },
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