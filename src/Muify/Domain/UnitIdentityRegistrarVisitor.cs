namespace Muify.Domain
{
    using System;
    using System.Collections.Generic;
    using Mu.Modelling;

    internal sealed class UnitIdentityRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Identity, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Identity identity)
        {
            throw new NotImplementedException();
        }
    }
}