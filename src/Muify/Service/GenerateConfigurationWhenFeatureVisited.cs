namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class GenerateConfigurationWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Poco options = feature._Value.Metadata.Options;

            if (feature._Value.Metadata.IsOutOfScope
                || (!options.IsUndefined
                    && (!options.IsPartial
                        || options.HasBase
                        || !options.Characteristics.IsClass
                        || !options.Characteristics.IsRecord)))
            {
                yield break;
            }

            string name = $"{feature._Value.Name}Options";

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom((Name: "Options", Qualifier: "Mu.Configuration"))
                    .Named(name)
                    .ForkOn(
                        _ => !options.IsUndefined,
                        @true: declaration => declaration
                            .WithExtensibility(Modifiers.Implicit)
                            .WithScope(Scopes.Unspecified),
                        @false: declaration => declaration))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature.Namespace}.{name}");
        }
    }
}