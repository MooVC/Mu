namespace Muify.Service
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Configuration;
    using static Muify.Service.GenerateRegisterIpcClientWhenFeatureVisited_Resources;

    internal sealed class GenerateRegisterIpcClientWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature) || feature._Value.Metadata.HasIpcClientRegistration)
            {
                yield break;
            }

            Symbol options = GenerateRegisterModeWhenFeatureVisited.GetOptionsSymbol(feature);
            string defaults = GenerateRegisterModeWhenFeatureVisited.GetDefaultOptions(feature);
            Symbol extensions = (Name: "ContainerExtensions", Qualifier: feature.Namespace);
            Symbol grpc = ($"I{feature._Value.Name}Service.IGrpc", feature.Namespace);
            Symbol contract = ($"I{feature._Value.Name}Service", feature.Namespace);
            Symbol client = ($"{feature._Value.Name}Service.Grpc.Client", feature.Namespace);

            Snippet configuration = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("container", ObserveContainerRequired),
                GenerateRegisterModeWhenFeatureVisited.GetGuard("configuration", ObserveConfigurationRequired),
                Snippet.Blank,
                $"var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<{Render(options)}>(",
                $"    configuration.GetSection(nameof({Render(options)}))) ?? {defaults};",
                Snippet.Blank,
                $"return {Render(extensions)}.RegisterIpcClient(container, options.Remote);")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterIpcClient(global::Microsoft.Extensions.Configuration.IConfiguration configuration)");

            Snippet unsupported = Snippet.From(Configuration.Options, $"throw new global::System.NotSupportedException({SymbolDisplay.FormatLiteral(ObserveIpcNotSupported, quote: true)});")
                .Block(Configuration.Options, "if (options.Ipc != global::Mu.Configuration.IpcType.Grpc)");

            Snippet registration = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("container", ObserveContainerRequired),
                "options ??= new global::Mu.Configuration.Remote();",
                Snippet.Blank,
                unsupported,
                Snippet.Blank,
                $"return global::Mu.Communications.Ipc.Grpc.Composition.ContainerExtensions.RegisterGrpcClient<{Render(grpc)}, {Render(contract)}>(",
                "    container,",
                "    options.Grpc,",
                $"    (client, scribe, timeout) => new {Render(client)}(client, scribe, timeout));")
                .Block(Configuration.Options, "public global::SimpleInjector.Container RegisterIpcClient(global::Mu.Configuration.Remote? options = default)");

            yield return GenerateRegisterModeWhenFeatureVisited.CreateExtension(
                feature,
                "RegisterIpcClient",
                Snippet.From(Configuration.Options, configuration, Snippet.Blank, registration));
        }
    }
}