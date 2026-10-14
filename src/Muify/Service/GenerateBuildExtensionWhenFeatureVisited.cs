namespace Muify.Service
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Configuration;
    using static Muify.Service.GenerateBuildExtensionWhenFeatureVisited_Resources;

    internal sealed class GenerateBuildExtensionWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            bool canGenerate = CanGenerateApplicationExtension(feature._Value);

            if (!CanGenerateBuilderExtension(feature._Value, canGenerate))
            {
                yield break;
            }

            string method = $"Build{feature._Value.Name}";
            string options = Render(GenerateRegisterModeWhenFeatureVisited.GetOptionsSymbol(feature));
            string defaults = GenerateRegisterModeWhenFeatureVisited.GetDefaultOptions(feature);
            Symbol registrar = (feature._Value.Name, feature.Namespace);
            Symbol service = ($"{feature._Value.Name}Service.Grpc.Service", feature.Namespace);

            if (!feature._Value.Metadata.HasBuilderBuildExtension)
            {
                yield return CreateBuilderExtension(feature.Namespace.ToString(), method);
            }

            if (feature._Value.Metadata.HasBuildExtension || !canGenerate)
            {
                yield break;
            }

            Snippet registration = Snippet.Empty;

            if (CanRegisterUnit(feature.Features.Unit._Value))
            {
                Symbol unit = (feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace);

                registration = Snippet.From(
                    Configuration.Options,
                    $"_ = global::Mu.Composition.ContainerExtensions.RegisterOnce<{Render(unit)}>(container, app.Configuration);")
                    .Block(Configuration.Options, "if (options.Mode == global::Mu.Configuration.ModeType.Hosted)");
            }

            Snippet mapping = Snippet.From(
                Configuration.Options,
                $"_ = global::Microsoft.AspNetCore.Builder.GrpcEndpointRouteBuilderExtensions.MapGrpcService<{Render(service)}>(app);")
                .Block(
                    Configuration.Options,
                    "if (options.Mode == global::Mu.Configuration.ModeType.Hosted && options.Hosted.Ipc == global::Mu.Configuration.IpcType.Grpc)");

            Snippet body = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("app", ObserveApplicationRequired),
                Snippet.Blank,
                $"var container = global::Mu.Composition.WebApplicationExtensions.GetMuContainer(app) ?? throw new global::System.InvalidOperationException({SymbolDisplay.FormatLiteral(ObserveContainerRequired, quote: true)});",
                Snippet.Blank,
                $"var options = global::Microsoft.Extensions.Configuration.ConfigurationBinder.Get<{options}>(app.Configuration.GetSection(nameof({options}))) ?? {defaults};",
                Snippet.Blank,
                registration,
                $"_ = global::Mu.Composition.ContainerExtensions.RegisterOnce<{Render(registrar)}>(container, app.Configuration);",
                "_ = global::Mu.Composition.ContainerExtensions.RegisterDefaults(container, app.Configuration);",
                Snippet.Blank,
                mapping,
                Snippet.Blank,
                "return app;")
                .Block(Configuration.Options, $"public global::Microsoft.AspNetCore.Builder.WebApplication {method}()");

            Snippet content = body
                .Block(Configuration.Options, "extension(global::Microsoft.AspNetCore.Builder.WebApplication app)")
                .Block(Configuration.Options, "public static partial class WebApplicationExtensions")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"namespace {feature.Namespace};")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, "#nullable enable");

            yield return new File(content, $"{feature.Namespace}.WebApplicationExtensions.{method}");
        }

        internal static File CreateBuilderExtension(string @namespace, string method)
        {
            Snippet composition = Snippet.From(
                Configuration.Options,
                $"return global::{@namespace}.WebApplicationExtensions.{method}(app);")
                .Block(Configuration.Options, "try");

            Snippet cleanup = Snippet.From(
                Configuration.Options,
                "app.DisposeAsync().AsTask().GetAwaiter().GetResult();",
                "throw;")
                .Block(Configuration.Options, "catch");

            Snippet body = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("builder", ObserveBuilderRequired),
                Snippet.Blank,
                "var app = global::Mu.Composition.WebApplicationBuilderExtensions.BuildMu(builder);",
                Snippet.Blank,
                composition,
                cleanup)
                .Block(Configuration.Options, $"public global::Microsoft.AspNetCore.Builder.WebApplication {method}()");

            Snippet content = body
                .Block(Configuration.Options, "extension(global::Microsoft.AspNetCore.Builder.WebApplicationBuilder builder)")
                .Block(Configuration.Options, "public static partial class WebApplicationBuilderExtensions")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"namespace {@namespace};")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, "#nullable enable");

            return new File(content, $"{@namespace}.WebApplicationBuilderExtensions.{method}");
        }

        private static bool CanGenerateApplicationExtension(Feature feature)
        {
            return GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature)
                && (feature.Metadata.HasRegistrar || feature.Metadata.IsPartial);
        }

        private static bool CanGenerateBuilderExtension(Feature feature, bool canGenerateApplicationExtension)
        {
            return !feature.Metadata.IsOutOfScope
                && (canGenerateApplicationExtension || feature.Metadata.HasBuildExtension);
        }

        private static bool CanRegisterUnit(Unit unit)
        {
            return unit.Metadata.HasRegistrar
                || (unit.Metadata.IsPartial && !unit.Metadata.IsOutOfScope);
        }
    }
}