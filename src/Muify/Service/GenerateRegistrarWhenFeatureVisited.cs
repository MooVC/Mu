namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;

    internal sealed class GenerateRegistrarWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!feature._Value.Metadata.IsPartial || feature._Value.Metadata.HasRegistrar || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Snippet registrations = GetRegistrations(feature);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record.WithRegistrar(registrations, feature._Value.Name))
                .From(feature.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "SimpleInjector"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature.Namespace}.{feature._Value.Name}.Registrar");
        }

        private static Snippet GetRegistrations(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var registrations = feature._Value.Metadata.References
                .Where(reference => !reference.Qualification.IsUnnamed
                    && (reference.HasBinder
                        || (reference.IsPartial
                            && !reference.Characteristics.IsUndefined
                            && !reference.Attributes.IsEmpty
                            && !reference.IsUndefined)))
                .OrderBy(reference => reference.Qualification.ToString(), StringComparer.Ordinal)
                .Select(reference => (Symbol)reference.Qualification)
                .Distinct()
                .Select(reference => reference.ToBinding())
                .ToList();

            registrations.Add(((Symbol)(feature._Value.Name, Qualifier: feature.Namespace)).ToBinding());

            foreach (Qualification registrar in feature._Value.Metadata.Registrars.Distinct())
            {
                registrations.Add($"{registrar.ToSnippet(Configuration.Options.Types)}.Register(configuration, container);");
            }

            if (GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature))
            {
                Symbol extensions = (Name: "ContainerExtensions", Qualifier: feature.Namespace);
                registrations.Add($"_ = {Configuration.Render(extensions)}.RegisterMode(container, configuration);");
            }

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }
    }
}