namespace Muify.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;

    internal sealed class UnitRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit unit)
        {
            if (!unit.Root.Metadata.HasComposition
             || unit.Value.Metadata.IsOutOfScope
             || unit.Value.Metadata.HasRegistrar)
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

            yield return new File(content, "Registrar");
        }

        private static Snippet ApplyRegistrars(Model.Graph.Areas.Area.Units.Unit unit)
        {
            return unit.Value.Metadata.Registrars
                .Select(registrar => $"{registrar.ToSnippet(Configuration.Options.Types)}.Register(configuration, container);")
                .ToSnippet(Configuration.Options);
        }
    }
}