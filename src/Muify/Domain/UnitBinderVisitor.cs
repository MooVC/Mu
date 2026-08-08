namespace Muify.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    internal sealed class UnitBinderVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (unit.Value.Metadata.IsOutOfScope || unit.Value.Metadata.HasBinder)
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(unit);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record.WithBinder(bindings, unit.Value.Name))
                .From(unit.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Binder");
        }

        private static Snippet ApplyBindings(Model.Graph.Areas.Area.Units.Unit unit)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({unit.Value.Name}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
                "meta.Add(1, \"Propositions\");",
                "meta.Add(2, \"Revision\");",
            };

            int index = 3;

            foreach (Attribute property in unit.Value.Attributes)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }
    }
}