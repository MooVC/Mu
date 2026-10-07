namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class GenerateBaseWhenUnitVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (!unit._Value.Metadata.IsPartial || unit._Value.Metadata.HasBase || unit._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            var content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom((Name: "Aggregate", Qualifier: "Mu.Modelling.State"))
                    .Named(unit._Value.Name))
                .From(unit.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, unit._Value.Name);
        }
    }
}