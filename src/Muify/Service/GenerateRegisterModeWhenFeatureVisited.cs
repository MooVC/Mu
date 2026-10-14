namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Configuration;
    using static Muify.Service.GenerateRegisterModeWhenFeatureVisited_Resources;

    internal sealed class GenerateRegisterModeWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature._Value.Metadata.HasModeRegistration || !CanGenerate(feature))
            {
                yield break;
            }

            string options = Render(GetOptionsSymbol(feature));
            string defaults = GetDefaultOptions(feature);
            Symbol extensions = (Name: "ContainerExtensions", Qualifier: feature.Namespace);

            Snippet configuration = Snippet
                .From(
                    Configuration.Options,
                    GetGuard("container", ObserveContainerRequired),
                    GetGuard("configuration", ObserveConfigurationRequired),
                    Snippet.Blank,
                    $"var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<{options}>(configuration.GetSection(nameof({options}))) ?? {defaults};",
                    Snippet.Blank,
                    $"return {Render(extensions)}.RegisterMode(container, options);")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterMode(global::Microsoft.Extensions.Configuration.IConfiguration configuration)");

            Snippet hosted = GetRegistrations(feature)
                .Append(Configuration.Options, $"_ = {Render(extensions)}.RegisterIpcHost(container, options.Hosted);")
                .Block(Configuration.Options, "if (options.Mode == global::Mu.Configuration.ModeType.Hosted)");

            Snippet remote = Snippet
                .From(Configuration.Options, $"_ = {Render(extensions)}.RegisterIpcClient(container, options.Remote);")
                .Block(Configuration.Options, "else if (options.Mode == global::Mu.Configuration.ModeType.Remote)");

            Snippet unsupported = Snippet
                .From(Configuration.Options, $"throw new global::System.NotSupportedException({SymbolDisplay.FormatLiteral(ObserveModeNotSupported, quote: true)});")
                .Block(Configuration.Options, "else");

            Snippet mode = Snippet
                .From(
                    Configuration.Options,
                    GetGuard("container", ObserveContainerRequired),
                    $"options ??= {defaults};",
                    Snippet.Blank,
                    hosted,
                    remote,
                    unsupported,
                    Snippet.Blank,
                    "return container;")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterMode(global::Mu.Configuration.Options? options = default)");

            Snippet body = Snippet.From(Configuration.Options, configuration, Snippet.Blank, mode);

            yield return CreateExtension(feature, "RegisterMode", body);
        }

        internal static bool CanGenerate(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            return CanGenerate(feature._Value);
        }

        internal static bool CanGenerate(Feature feature)
        {
            Poco options = feature.Metadata.Options;

            return !feature.Metadata.IsOutOfScope
                && options.IsAccessible
                && CanUseOptions(options);
        }

        internal static File CreateExtension(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, string method, Snippet body)
        {
            Snippet content = body
                .Block(Configuration.Options, "extension(global::SimpleInjector.Container container)")
                .Block(Configuration.Options, "public static partial class ContainerExtensions")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"namespace {feature.Namespace};")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, "#nullable enable");

            return new File(content, $"{feature.Namespace}.ContainerExtensions.{method}");
        }

        internal static Symbol GetOptionsSymbol(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Poco options = feature._Value.Metadata.Options;

            return options.IsUndefined
                ? (Symbol)(Name: $"{feature._Value.Name}Options", Qualifier: feature.Namespace)
                : (Symbol)options.Qualification;
        }

        internal static string GetGuard(string parameter, string message)
        {
            return $"_ = global::Ardalis.GuardClauses.GuardClauseExtensions.Null(global::Ardalis.GuardClauses.Guard.Against, {parameter}, nameof({parameter}), message: {SymbolDisplay.FormatLiteral(message, quote: true)});";
        }

        internal static string GetDefaultOptions(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            Symbol options = feature._Value.Metadata.Options.HasParameterlessConstructor
                ? GetOptionsSymbol(feature)
                : (Symbol)(Name: "Options", Qualifier: "Mu.Configuration");

            return $"new {Render(options)}()";
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

        private static bool CanExtendOptions(Poco options)
        {
            return options.IsPartial
                && !options.HasBase
                && options.Characteristics.IsClass
                && options.Characteristics.IsRecord;
        }

        private static bool CanUseOptions(Poco options)
        {
            return options.IsUndefined
                || options.HasOptionsBase
                || CanExtendOptions(options);
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

        private static string DefineMutationalServiceRegistration(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature, List<string> allocatorRegistrations)
        {
            Unit unit = feature.Features.Unit._Value;
            Symbol identity = unit.Identity.GetSymbol(feature.Features.Unit.Namespace);

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

            if (ShouldRegisterGuidAllocator(feature._Value, unit))
            {
                Symbol allocator = Symbol.Undefined
                    .Named((Name: "IAllocator", Qualifier: "Mu.Modelling.Services"))
                    .WithArguments(identity);

                Symbol implementation = (Name: "GuidAllocator", Qualifier: "Mu.Modelling.Services");
                string predicate = $"context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof({Render(service)})";

                allocatorRegistrations.Add($"container.RegisterConditional<{Render(allocator)}, {Render(implementation)}>({Render(LifeStyles.Scoped)}, {predicate});");
            }

            return GetRegistration(contract, service);
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

        private static string GetRegistration(Symbol contract, Symbol service)
        {
            return $"container.Register<{Render(contract)}, {Render(service)}>({Render(LifeStyles.Scoped)});";
        }

        private static Snippet GetRegistrations(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            var registrations = new List<string>();
            var allocatorRegistrations = new List<string>();

            if (ShouldRegisterDefaultHandler(feature._Value))
            {
                registrations.Add(DefineMutationalHandlerRegistration(feature));

                if (ShouldRegisterDefaultService(feature._Value))
                {
                    registrations.Add(DefineMutationalServiceRegistration(feature, allocatorRegistrations));
                }
            }

            registrations.AddRange(allocatorRegistrations);

            if (feature._Value.Type.IsMutational)
            {
                ApplyCollections(feature, registrations);
                registrations.Add(DefineMutationalRootRegistration(feature));
            }

            return Snippet.From(Configuration.Options, registrations.ToArray());
        }

        private static bool ShouldRegisterDefaultHandler(Feature feature)
        {
            return feature.Metadata.Handler.IsUnnamed
                && (feature.Type.IsMutational || feature.Results.Length > 0);
        }

        private static bool ShouldRegisterDefaultService(Feature feature)
        {
            return feature.Type.IsMutational && feature.Metadata.Service.IsUnnamed;
        }

        private static bool ShouldRegisterGuidAllocator(Feature feature, Unit unit)
        {
            return feature.Mutational.Type.IsCreational
                && unit.Metadata.Allocator.IsUndefined
                && unit.Identity.IsType
                && unit.Identity.Type.Equals((Qualification)typeof(Guid));
        }
    }
}