namespace Muify.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;

    internal sealed class GenerateRegistrarWhenUnitVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (!unit.Value.Metadata.IsPartial || unit.Value.Metadata.HasRegistrar || unit.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Snippet registrations = ApplyRegistrars(unit);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record.WithRegistrar(registrations, unit.Value.Name))
                .From(unit.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "SimpleInjector"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{unit.Value.Name}.Registrar");
        }

        private static Snippet ApplyRegistrars(Model.Graph.Areas.Area.Units.Unit unit)
        {
            var registrations = unit.Value.Components
                .Where(component => !component.Metadata.IsOutOfScope)
                .OrderBy(component => component.Name.ToString(), StringComparer.Ordinal)
                .Select(component => (Symbol)(component.Name, Qualifier: unit.Namespace))
                .Distinct()
                .Select(component => component.ToBinding())
                .ToList();

            Symbol self = (unit.Value.Name, Qualifier: unit.Namespace);

            registrations.Add(self.ToBinding());

            registrations.AddRange(unit.Value.Metadata.Registrars
                .Select(registrar => $"{registrar.ToSnippet(Configuration.Options.Types)}.Register(configuration, container);"));

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }
    }
}