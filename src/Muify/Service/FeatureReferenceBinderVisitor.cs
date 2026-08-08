namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    internal sealed class FeatureReferenceBinderVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco poco)
        {
            if (!poco.Value.IsPartial
              || poco.Value.Qualification.IsUnnamed
              || poco.Value.Characteristics.IsUndefined
              || poco.Value.Attributes.IsEmpty
              || poco.Value.IsUndefined)
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(poco);

            string content = Builder
                .New<Definition>()
                .ForkOn(
                    _ => poco.Value.Characteristics.IsRecord,
                    @true: type => type.For<Record>(record => record.WithBinder(bindings, poco.Value.Qualification.Moniker)),
                    @false: next => next.ForkOn(
                        _ => poco.Value.Characteristics.IsStruct,
                        @true: type => type.For<Struct>(@struct => @struct.WithBinder(bindings, poco.Value.Characteristics, poco.Value.Qualification.Moniker)),
                        @false: type => type.For<Class>(@class => @class.WithBinder(bindings, poco.Value.Qualification.Moniker))))
                .From(poco.Value.Qualification.Qualifier)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Binder");
        }

        private static Snippet ApplyBindings(Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco poco)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({poco.Value.Qualification.Moniker}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
            };

            int index = 4;

            foreach (Attribute property in poco.Value.Attributes)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }
    }
}