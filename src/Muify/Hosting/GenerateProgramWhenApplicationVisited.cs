namespace Muify.Hosting
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using Mu.Modelling;

    internal sealed class GenerateProgramWhenApplicationVisited
        : IModelVisitor<Model.Graph.Metadata, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Metadata metadata)
        {
            if (!metadata._Value.IsExecutable || metadata._Value.HasProgram || string.IsNullOrEmpty(metadata._Value.BuildName))
            {
                yield break;
            }

            string @namespace = metadata._Value.ApplicationNamespace;
            string method = metadata._Value.BuildName;

            Snippet body = Snippet.From(
                Configuration.Options,
                "var builder = global::Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);",
                $"await using var app = global::{@namespace}.WebApplicationBuilderExtensions.{method}(builder);",
                "await app.RunAsync().ConfigureAwait(false);")
                .Block(Configuration.Options, "public static async global::System.Threading.Tasks.Task Main(string[] args)");

            Snippet content = body
                .Block(Configuration.Options, "internal static class Program")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"namespace {@namespace};")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, "#nullable enable");

            yield return new File(content, $"{@namespace}.Program");
        }
    }
}