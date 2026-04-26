namespace Muify.Domain
{
    using System.Collections.Generic;
    using Graphify;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class UnitInspector
        : IInspector<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (unit.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            var content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom((Name: "Aggregate", Qualifier: "Mu.Modelling.State"))
                    .Named(unit.Value.Name))
                .ToSnippet(Configuration.Options);

            yield return new File(content, unit.Value.Name);
        }
    }
}