namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Mu.Modelling;

    internal sealed class FeatureServiceContractVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature instance)
        {
            throw new NotImplementedException();
        }
    }
}