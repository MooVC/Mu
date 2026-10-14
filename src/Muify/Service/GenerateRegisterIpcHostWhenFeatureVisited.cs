namespace Muify.Service
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Configuration;
    using static Muify.Service.GenerateRegisterIpcHostWhenFeatureVisited_Resources;

    internal sealed class GenerateRegisterIpcHostWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature) || feature._Value.Metadata.HasIpcHostRegistration)
            {
                yield break;
            }

            Symbol options = GenerateRegisterModeWhenFeatureVisited.GetOptionsSymbol(feature);
            string defaults = GenerateRegisterModeWhenFeatureVisited.GetDefaultOptions(feature);
            Symbol extensions = (Name: "ContainerExtensions", Qualifier: feature.Namespace);
            Symbol service = ($"{feature._Value.Name}Service.Grpc.Service", feature.Namespace);

            Snippet configuration = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("container", ObserveContainerRequired),
                GenerateRegisterModeWhenFeatureVisited.GetGuard("configuration", ObserveConfigurationRequired),
                Snippet.Blank,
                $"var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<{Render(options)}>(",
                $"    configuration.GetSection(nameof({Render(options)}))) ?? {defaults};",
                Snippet.Blank,
                $"return {Render(extensions)}.RegisterIpcHost(container, options.Hosted);")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterIpcHost(global::Microsoft.Extensions.Configuration.IConfiguration configuration)");

            Snippet unsupported = Snippet.From(Configuration.Options, $"throw new global::System.NotSupportedException({SymbolDisplay.FormatLiteral(ObserveIpcNotSupported, quote: true)});")
                .Block(Configuration.Options, "if (options.Ipc != global::Mu.Configuration.IpcType.Grpc)");

            Snippet registration = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("container", ObserveContainerRequired),
                "options ??= new global::Mu.Configuration.Hosted();",
                Snippet.Blank,
                unsupported,
                Snippet.Blank,
                $"container.Register<{Render(service)}>({Render(LifeStyles.Scoped)});",
                "container.RegisterConditional(",
                "    typeof(global::Mu.Communications.Mediation.IMediator),",
                "    _ => typeof(global::Mu.Communications.Mediation.InMemoryMediator),",
                $"    {Render(LifeStyles.Scoped)},",
                $"    context => !context.Handled && context.HasConsumer && context.Consumer.ImplementationType == typeof({Render(service)}));",
                Snippet.Blank,
                $"return global::Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions.RegisterGrpcHost<{Render(service)}>(container, options.Grpc);")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterIpcHost(global::Mu.Configuration.Hosted? options = default)");

            yield return GenerateRegisterModeWhenFeatureVisited.CreateExtension(
                feature,
                "RegisterIpcHost",
                Snippet.From(Configuration.Options, configuration, Snippet.Blank, registration));
        }
    }
}