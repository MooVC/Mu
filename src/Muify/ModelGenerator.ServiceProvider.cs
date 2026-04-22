namespace Muify
{
    using System;
    using System.Collections.Generic;

    public partial class ModelGenerator
    {
        private sealed class ServiceProvider
            : IServiceProvider
        {
            private readonly IDictionary<Type, object> _services = new Dictionary<Type, object>
            {
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