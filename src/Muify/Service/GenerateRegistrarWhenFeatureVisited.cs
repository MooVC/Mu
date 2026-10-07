namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using static Muify.Configuration;

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

            yield return new File(content, $"{feature._Value.Name}.Registrar");
        }

        private static void ApplyRegistrars(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, List<string> registrations)
        {
            foreach (Qualification registrar in feature._Value.Metadata.Registrars.Distinct())
            {
                registrations.Add($"{registrar.ToSnippet(Configuration.Options.Types)}.Register(configuration, container);");
            }
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

            if (feature._Value.Metadata.Handler.IsUnnamed
            && (feature._Value.Type.IsMutational || feature._Value.Results.Length > 0))
            {
                registrations.Add(DefineMutationalHandlerRegistration(feature));

                if (feature._Value.Type.IsMutational && feature._Value.Metadata.Service.IsUnnamed)
                {
                    registrations.Add(DefineMutationalServiceRegistration(feature));
                }
            }

            ApplyRegistrars(feature, registrations);

            if (feature._Value.Type.IsMutational)
            {
                ApplyCollections(feature, registrations);
                registrations.Add(DefineMutationalRootRegistration(feature));
            }

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }

        private static void ApplyCollections(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, List<string> registrations)
        {
            Symbol aggregate = (feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace);
            Symbol request = (feature._Value.Name, feature.Namespace);
            Symbol fact = (feature._Value.Mutational.Fact, feature.Namespace);

            Symbol invariants = Symbol.Undefined
                .Named((Name: "IInvariant", Qualifier: "Mu.Modelling.Integrity"))
                .WithArguments(aggregate)
                .WithArguments(request);

            Symbol transforms = Symbol.Undefined
                .Named((Name: "ITransform", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(aggregate)
                .WithArguments(fact);

            IEnumerable<Qualification> implementations = feature._Value.Metadata.Transforms.IsDefaultOrEmpty
                ? new Qualification[] { (Name: "Transform", Qualifier: feature.Namespace) }
                : feature._Value.Metadata.Transforms.AsEnumerable();

            registrations.Add(GetCollectionRegistration(invariants, feature._Value.Metadata.Invariants));
            registrations.Add(GetCollectionRegistration(transforms, implementations));
        }

        private static string GetCollectionRegistration(Symbol contract, IEnumerable<Qualification> implementations)
        {
            string[] types = implementations
                .Distinct()
                .OrderBy(implementation => implementation.ToString(), StringComparer.Ordinal)
                .Select(implementation => $"typeof({Render((Symbol)implementation)})")
                .ToArray();

            Symbol array = (Name: "Array", Qualifier: "System");
            Symbol type = (Name: "Type", Qualifier: "System");

            string collection = types.Length == 0
                ? $"{Render(array)}.Empty<{Render(type)}>()"
                : $"new[] {{ {string.Join(", ", types)} }}";

            return $"container.Collection.Register<{Render(contract)}>({collection}, {Render(LifeStyles.Scoped)});";
        }

        private static string DefineMutationalHandlerRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Qualification DefineMutationalResult()
            {
                return feature._Value.Mutational.Type.IsCreational
                    ? feature.Features.Unit._Value.Identity.GetSymbol(feature.Features.Unit.Namespace).Name
                    : (Name: "Revision", Qualifier: "Mu.Modelling.State");
            }

            Symbol DefineUseCase(Symbol usecase)
            {
                return usecase.Named((feature._Value.Name, feature.Namespace));
            }

            Symbol DefineResult(Symbol outcome)
            {
                Qualification result = feature._Value.Results.Length == 0
                    ? DefineMutationalResult()
                    : (Name: $"{feature._Value.Name}.Result", Qualifier: feature.Namespace);

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

        private static string DefineMutationalRootRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Symbol aggregate = (feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace);
            Symbol request = (feature._Value.Name, feature.Namespace);
            Symbol fact = (feature._Value.Mutational.Fact, feature.Namespace);

            Symbol contract = Symbol.Undefined
                .Named((Name: "IRoot", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(aggregate, request);

            Symbol implementation = feature._Value.Metadata.Root.IsUnnamed
                ? Symbol.Undefined
                    .Named((Name: "Root", Qualifier: "Mu.Modelling.Services"))
                    .WithArguments(aggregate, fact, request)
                : (Symbol)feature._Value.Metadata.Root;

            return GetRegistration(contract, implementation);
        }

        private static string DefineMutationalServiceRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Symbol identity = feature.Features.Unit._Value.Identity.GetSymbol(feature.Features.Unit.Namespace);

            Symbol contract = Symbol.Undefined
                .Named((Name: "IService", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(usecase => usecase.Named((feature._Value.Name, feature.Namespace)))
                .WithArguments(identity);

            Symbol service = Symbol.Undefined
                .Named(qualification => qualification
                    .From("Mu.Modelling.Services")
                    .ForkOn(
                        _ => feature._Value.Mutational.Type.IsCreational,
                        @true: creational => creational.KnownAs("CreationalService"),
                        @false: transitional => transitional.KnownAs("TransitionalService")))
                .WithArguments(aggregate => aggregate.Named((feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace)))
                .WithArguments(identity)
                .WithArguments(usecase => usecase.Named((feature._Value.Name, feature.Namespace)));

            return GetRegistration(contract, service);
        }

        private static string GetRegistration(Symbol contract, Symbol service)
        {
            return $"container.Register<{Render(contract)}, {Render(service)}>({Render(LifeStyles.Scoped)});";
        }
    }
}