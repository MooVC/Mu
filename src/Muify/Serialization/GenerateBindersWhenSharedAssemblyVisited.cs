namespace Muify.Serialization
{
    using System.Collections.Generic;
    using Mu.Modelling;
    using Muify.Service;

    internal sealed class GenerateBindersWhenSharedAssemblyVisited
        : IModelVisitor<Model.Graph.Metadata, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Metadata metadata)
        {
            if (!metadata._Value.IsSharedProject)
            {
                yield break;
            }

            foreach (Poco type in metadata._Value.SharedTypes)
            {
                foreach (File binder in GenerateBinderWhenFeatureReferenceVisited.Generate(type))
                {
                    yield return binder;
                }
            }
        }
    }
}