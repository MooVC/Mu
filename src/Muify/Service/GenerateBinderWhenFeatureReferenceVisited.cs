namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    internal sealed class GenerateBinderWhenFeatureReferenceVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco poco)
        {
            if (!poco._Value.IsPartial
              || poco._Value.HasBinder
              || poco._Value.Qualification.IsUnnamed
              || poco._Value.Characteristics.IsUndefined
              || poco._Value.Attributes.IsEmpty
              || poco._Value.IsUndefined)
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(poco);

            string content = Builder
                .New<Definition>()
                .ForkOn(
                    _ => poco._Value.Characteristics.IsRecord,
                    @true: type => type.For<Record>(record => record.WithBinder(bindings, poco._Value.Qualification.Moniker)),
                    @false: next => next.ForkOn(
                        _ => poco._Value.Characteristics.IsStruct,
                        @true: type => type.For<Struct>(@struct => @struct.WithBinder(bindings, poco._Value.Characteristics, poco._Value.Qualification.Moniker)),
                        @false: type => type.For<Class>(@class => @class.WithBinder(bindings, poco._Value.Qualification.Moniker))))
                .From(poco._Value.Qualification.Qualifier)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{poco._Value.Qualification.Qualifier}.{poco._Value.Qualification.Moniker}.Binder");
        }

        private static Snippet ApplyBindings(Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco poco)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({poco._Value.Qualification.Moniker}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
            };

            int index = 4;

            foreach (Attribute property in poco._Value.Attributes)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }
    }
}