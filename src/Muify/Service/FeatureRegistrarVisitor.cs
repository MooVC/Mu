namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using static Muify.Configuration;

    internal sealed class FeatureRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasRegistrar || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Snippet registrations = GetRegistrations(feature);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record.WithRegistrar(registrations, feature.Value.Name))
                .From(feature.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "SimpleInjector"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature.Value.Name}.Registrar");
        }

        private static void ApplyRegistrars(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, List<string> registrations)
        {
            foreach (Qualification registrar in feature.Value.Metadata.Registrars)
            {
                registrations.Add($"{registrar.ToSnippet(Configuration.Options.Types)}.Register(configuration, container);");
            }
        }

        private static Snippet GetRegistrations(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var registrations = new List<string>();

            if (feature.Value.Metadata.Handler.IsUnnamed
            && (feature.Value.Type.IsMutational || feature.Value.Results.Length > 0))
            {
                registrations.Add(DefineMutationalHandlerRegistration(feature));

                if (feature.Value.Type.IsMutational && feature.Value.Metadata.Service.IsUnnamed)
                {
                    registrations.Add(DefineMutationalServiceRegistration(feature));
                }
            }

            ApplyRegistrars(feature, registrations);

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }

        private static string DefineMutationalHandlerRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Qualification DefineMutationalResult()
            {
                return feature.Value.Mutational.Type.IsCreational
                    ? feature.Features.Unit.Value.Identity.GetSymbol(feature.Features.Unit.Namespace).Name
                    : (Name: "Revision", Qualifier: "Mu.Modelling.State");
            }

            Symbol DefineUseCase(Symbol usecase)
            {
                return usecase.Named((feature.Value.Name, feature.Namespace));
            }

            Symbol DefineResult(Symbol outcome)
            {
                Qualification result = feature.Value.Results.Length == 0
                    ? DefineMutationalResult()
                    : (Name: $"{feature.Value.Name}.Result", Qualifier: feature.Namespace);

                return outcome.Named(result);
            }

            Symbol contract = Symbol.Undefined
                .Named((Name: "IHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(DefineUseCase)
                .WithArguments(DefineResult);

            Symbol service = Symbol.Undefined
                .Named((Name: "ServiceHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(DefineUseCase)
                .WithArguments(DefineResult);

            return GetRegistration(contract, service);
        }

        private static string DefineMutationalServiceRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Symbol identity = feature.Features.Unit.Value.Identity.GetSymbol(feature.Features.Unit.Namespace);

            Symbol contract = Symbol.Undefined
                .Named((Name: "IService", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(usecase => usecase.Named((feature.Value.Name, feature.Namespace)))
                .WithArguments(identity);

            Symbol service = Symbol.Undefined
                .Named(qualification => qualification
                    .From("Mu.Modelling.Services")
                    .ForkOn(
                        _ => feature.Value.Mutational.Type.IsCreational,
                        @true: creational => creational.KnownAs("CreationalService"),
                        @false: transitional => transitional.KnownAs("TransitionalService")))
                .WithArguments(aggregate => aggregate.Named((feature.Features.Unit.Value.Name, feature.Features.Unit.Namespace)))
                .WithArguments(identity)
                .WithArguments(usecase => usecase.Named((feature.Value.Name, feature.Namespace)));

            return GetRegistration(contract, service);
        }

        private static string GetRegistration(Symbol contract, Symbol service)
        {
            return $"container.Register<{Render(contract)}, {Render(service)}>({Render(LifeStyles.Scoped)});";
        }
    }
}