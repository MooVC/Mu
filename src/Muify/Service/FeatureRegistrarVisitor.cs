namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Configuration;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class FeatureRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.IsOutOfScope || feature.Value.Metadata.HasRegistrar)
            {
                yield break;
            }

            Symbol configuration = (Name: "IConfiguration", Qualifier: "Microsoft.Extensions.Configuration");
            Symbol container = (Name: "Container", Qualifier: "SimpleInjector");

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .Implements((Name: "IRegistrar", Qualifier: "Mu.Composition"), registrar => registrar.WithArguments(container))
                    .Named(feature.Value.Name)
                    .WithMethods(register => register
                        .Accepts((Name: "Configuration", Type: configuration))
                        .Accepts((Name: "Container", Type: container))
                        .Named("Register")
                        .Returns(result => result
                            .OfType(container)
                            .WithMode(Result.Modes.Synchronous))
                        .WithExtensibility(Modifiers.Static)
                        .WithBody(GetRegistrations(feature))))
                .From(feature.Namespace)
                .Referencing(container.Name.Qualifier)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Registrar");
        }

        private static Snippet GetRegistrations(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var registrations = new List<string>();

            if (feature.Value.Metadata.Handler.IsUnnamed && !feature.Features.Unit.Value.Identity.IsUnnamed)
            {
                if (feature.Value.Type.IsMutational || feature.Value.Results.Length > 0)
                {
                    registrations.Add(GetMutationalHandlerRegistration(feature));

                    if (feature.Value.Type.IsMutational && feature.Value.Metadata.Serivce.IsUnnamed)
                    {
                        registrations.Add(GetMutationalServiceRegistration(feature));
                    }

                    registrations.Add(string.Empty);
                }
            }

            registrations.Add("return container;");

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }

        private static string GetMutationalHandlerRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Qualification GetMutationalResult()
            {
                return feature.Value.Mutational.Type.IsCreational
                    ? feature.Features.Unit.Value.Identity
                    : (Name: "Revision", Qualifier: "Mu.Modelling.State");
            }

            Symbol GetUseCase(Symbol usecase)
            {
                return usecase.Named((feature.Value.Name, feature.Namespace));
            }

            Symbol GetResult(Symbol outcome)
            {
                Qualification result = feature.Value.Results.Length == 0
                    ? GetMutationalResult()
                    : (Name: $"{feature.Value.Name}.Result", Qualifier: feature.Namespace);

                return outcome.Named(result);
            }

            Symbol contract = Symbol.Undefined
                .Named((Name: "IHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(GetUseCase)
                .WithArguments(GetResult);

            Symbol service = Symbol.Undefined
                .Named((Name: "ServiceHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(GetUseCase)
                .WithArguments(GetResult);

            return GetRegistration(contract, service);
        }

        private static string GetMutationalServiceRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Symbol contract = Symbol.Undefined
                .Named((Name: "IService", Qualifier: "Mu.Modelling.Services"))
                .WithArguments(usecase => usecase.Named((feature.Value.Name, feature.Namespace)))
                .WithArguments(result => result.Named(feature.Features.Unit.Value.Identity));

            Symbol service = Symbol.Undefined
                .Named(qualification => qualification
                    .From("Mu.Modelling.Services")
                    .ForkOn(
                        _ => feature.Value.Mutational.Type.IsCreational,
                        @true: creational => creational.KnownAs("CreationalService"),
                        @false: transitional => transitional.KnownAs("TransitionalService")))
                .WithArguments(aggregate => aggregate.Named((feature.Features.Unit.Value.Name, feature.Features.Unit.Namespace)))
                .WithArguments(identity => identity.Named(feature.Features.Unit.Value.Identity))
                .WithArguments(usecase => usecase.Named((feature.Value.Name, feature.Namespace)));

            return GetRegistration(contract, service);
        }

        private static string GetRegistration(Symbol contract, Symbol service)
        {
            return $"container.Register<{Render(contract)}, {Render(service)}>({Render(LifeStyles.Scoped)});";
        }
    }
}