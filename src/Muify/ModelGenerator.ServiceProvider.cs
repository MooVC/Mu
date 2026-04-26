namespace Muify
{
    using System;
    using System.Collections.Generic;
    using Graphify;
    using Mu.Modelling;
    using Muify.Domain;

    public partial class ModelGenerator
    {
        private sealed class ServiceProvider
            : IServiceProvider
        {
            private static readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
                { typeof(IInspector<Model.Graph.Areas.Area.Units.Unit, File>), new UnitInspector() },
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