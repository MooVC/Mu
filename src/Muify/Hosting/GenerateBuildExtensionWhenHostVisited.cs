namespace Muify.Hosting
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using Mu.Modelling;
    using Muify.Service;
    using static Muify.Hosting.GenerateBuildExtensionWhenHostVisited_Resources;

    internal sealed class GenerateBuildExtensionWhenHostVisited
        : IModelVisitor<Model.Graph.Metadata, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Metadata metadata)
        {
            if (!metadata._Value.IsHost)
            {
                yield break;
            }

            string @namespace = metadata._Value.ApplicationNamespace;
            string method = metadata._Value.BuildName;

            if (!metadata._Value.HasBuilderBuildExtension)
            {
                yield return GenerateBuildExtensionWhenFeatureVisited.CreateBuilderExtension(@namespace, method);
            }

            if (metadata._Value.HasApplicationBuildExtension)
            {
                yield break;
            }

            string[] features = metadata._Value.Features
                .Select(feature => $"_ = global::{feature.Qualifier}.WebApplicationExtensions.Build{feature.Moniker}(app);")
                .ToArray();

            Snippet body = Snippet.From(
                Configuration.Options,
                GenerateRegisterModeWhenFeatureVisited.GetGuard("app", ObserveApplicationRequired),
                Snippet.Blank,
                $"var container = global::Mu.Composition.WebApplicationExtensions.GetMuContainer(app) ?? throw new global::System.InvalidOperationException({SymbolDisplay.FormatLiteral(ObserveContainerRequired, quote: true)});",
                Snippet.Blank,
                Snippet.From(Configuration.Options, features),
                "_ = global::Mu.Composition.ContainerExtensions.RegisterDefaults(container, app.Configuration);",
                Snippet.Blank,
                "return app;")
                .Block(Configuration.Options, $"public global::Microsoft.AspNetCore.Builder.WebApplication {method}()");

            Snippet content = body
                .Block(Configuration.Options, "extension(global::Microsoft.AspNetCore.Builder.WebApplication app)")
                .Block(Configuration.Options, "public static partial class WebApplicationExtensions")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"namespace {@namespace};")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, "#nullable enable");

            yield return new File(content, $"{@namespace}.WebApplicationExtensions.{method}");
        }
    }
}