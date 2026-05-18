namespace Muify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graphify;
    using Mu.Modelling;

    public partial class ModelGenerator
    {
        private sealed class CollectionVisitor<T>
            : IModelVisitor<T, File>
            where T : class, IGraph<Model>
        {
            private readonly Type[] _visitors;

            public CollectionVisitor(params Type[] visitors)
            {
                _visitors = visitors;
            }

            public IEnumerable<File> Observe(T instance)
            {
                return _visitors
                    .Select(visitor => Activator.CreateInstance(visitor))
                    .Cast<IModelVisitor<T, File>>()
                    .SelectMany(visitor => visitor.Observe(instance));
            }
        }
    }
}