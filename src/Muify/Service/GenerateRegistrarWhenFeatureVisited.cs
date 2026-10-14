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
            if (!CanGenerateRegistrar(feature._Value))
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

        private static bool CanBindReference(Poco reference)
        {
            return reference.IsAccessible
                && !reference.Qualification.IsUnnamed
                && (reference.HasBinder || reference.HasDomainBinder || GenerateBinderWhenFeatureReferenceVisited.CanGenerateBinder(reference));
        }

        private static bool CanGenerateRegistrar(Feature feature)
        {
            return feature.Metadata.IsPartial
                && !feature.Metadata.HasRegistrar
                && !feature.Metadata.IsOutOfScope;
        }

        private static string GetBinding(Poco reference)
        {
            return ((Symbol)reference.Qualification).ToBinding(whenUndefined: true);
        }

        private static Snippet GetRegistrations(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var registrations = feature._Value.Metadata.References
                .Where(CanBindReference)
                .OrderBy(reference => reference.Qualification.ToString(), StringComparer.Ordinal)
                .GroupBy(reference => (Symbol)reference.Qualification)
                .Select(group => group.First())
                .Select(GetBinding)
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