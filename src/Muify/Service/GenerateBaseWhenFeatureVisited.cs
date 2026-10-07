namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class GenerateBaseWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!feature._Value.Metadata.IsPartial || feature._Value.Metadata.HasBase || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            string name = feature._Value.Type.IsMutational
                ? feature._Value.Mutational.Type.ToString()
                : "Query";

            Symbol aggregate = (feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace);

            Token[] arguments = feature._Value.Type.IsMutational && feature._Value.Mutational.Type.IsTransitional
                ? new Token[] { aggregate, feature.Features.Unit._Value.Identity.GetSymbol(feature.Features.Unit.Namespace) }
                : new Token[] { aggregate };

            var content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom(@base => @base
                        .Named((Name: name, Qualifier: "Mu.Modelling.Behavior"))
                        .WithGenerics(arguments))
                    .Named(feature._Value.Name))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, feature._Value.Name);
        }
    }
}