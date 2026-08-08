namespace Muify.Domain
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    internal sealed class ComponentBinderVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Components.Component, File>,
          IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Components.Component, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Components.Component component)
        {
            return Generate(component.Value, component.Namespace);
        }

        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Components.Component component)
        {
            return Generate(component.Value, component.Namespace);
        }

        private static IEnumerable<File> Generate(Component component, Qualifier @namespace)
        {
            if (component.Metadata.HasBinder || component.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(component);

            string content = Builder
                .New<Definition>()
                .ForkOn(
                    _ => component.Metadata.Characteristics.IsRecord,
                    @true: type => type.For<Record>(record => record.WithBinder(bindings, component.Name)),
                    @false: next => next.ForkOn(
                        _ => component.Metadata.Characteristics.IsStruct,
                        @true: type => type.For<Struct>(@struct => @struct.WithBinder(bindings, component.Metadata.Characteristics, component.Name)),
                        @false: type => type.For<Class>(@class => @class.WithBinder(bindings, component.Name))))
                .From(@namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Binder");
        }

        private static Snippet ApplyBindings(Component component)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({component.Name}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
            };

            int index = 1;

            if (!component.Identifier.IsUndefined)
            {
                bindings.Add($"meta.Add({index++}, \"{component.Identifier.Name}\");");
            }

            foreach (Attribute property in component.Attributes)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }
    }
}