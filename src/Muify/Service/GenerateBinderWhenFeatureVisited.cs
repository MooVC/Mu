namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Parameter = Mu.Modelling.Parameter;

    internal sealed class GenerateBinderWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!feature._Value.Metadata.IsPartial || feature._Value.Metadata.HasBinder || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(feature);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record.WithBinder(bindings, feature._Value.Name))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature._Value.Name}.Binder");
        }

        private static Snippet ApplyBindings(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({feature._Value.Name}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
                "meta.Add(1, \"Identity\");",
                "meta.Add(2, \"Proposed\");",
                "meta.Add(3, \"Model\");",
            };

            int index = 4;

            foreach (Parameter property in feature._Value.Parameters)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name.ToSnippet(Identifier.Options.Pascal)}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }
    }
}