namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        private static ImmutableArray<Qualification> GetHostedFeatures(this Compilation compilation)
        {
            const int FeatureSegments = 5;

            return compilation.SourceModule.ReferencedAssemblySymbols
                .Select(assembly => (Assembly: assembly, Segments: assembly.Identity.Name.Split('.')))
                .Where(reference => reference.Segments.Length == FeatureSegments && reference.Segments[FeatureSegments - 1] != "Host")
                .Select(reference => reference.Assembly.GetTypeByMetadataName($"{reference.Assembly.Identity.Name}.{reference.Segments[FeatureSegments - 1]}"))
                .Where(request => request is object
                    && request.Arity == 0
                    && request.HasUseCaseBase()
                    && compilation.IsSymbolAccessibleWithin(request, compilation.Assembly)
                    && request.ContainingNamespace.GetTypeMembers("WebApplicationExtensions").Any(extensions =>
                        extensions.HasApplicationExtension($"Build{request.Name}", "Microsoft.AspNetCore.Builder.WebApplication", compilation)))
                .Select(request => request.ToQualification())
                .Distinct()
                .OrderBy(feature => $"{feature.Qualifier}.{feature.Moniker}", StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static bool HasHostSuffix(string name)
        {
            string[] segments = name.Split('.');

            return segments.Length >= 2 && segments[segments.Length - 1] == "Host";
        }

        private static bool HasProgram(this Compilation compilation)
        {
            return compilation.GetEntryPoint(CancellationToken.None) is object
                || compilation.SyntaxTrees.Any(tree =>
                    string.Equals(Path.GetFileName(tree.FilePath), "Program.cs", StringComparison.OrdinalIgnoreCase)
                    || tree.GetRoot().DescendantNodes().OfType<GlobalStatementSyntax>().Any());
        }

        private static Model WithApplicationMetadata(
            this Compilation compilation,
            Model model,
            ImmutableArray<string> assemblies,
            string applicationNamespace,
            string buildName,
            bool isHost,
            ImmutableArray<Qualification> features)
        {
            INamedTypeSymbol applicationExtensions = compilation.Assembly.GetTypeByMetadataName($"{applicationNamespace}.WebApplicationExtensions");
            INamedTypeSymbol builderExtensions = compilation.Assembly.GetTypeByMetadataName($"{applicationNamespace}.WebApplicationBuilderExtensions");

            return model.WithMetadata(metadata => metadata
                .Enumerate((assembly, subject) => subject.WithAssemblies(assembly), assemblies)
                .Enumerate((feature, subject) => subject.WithFeatures(feature), features)
                .WithApplicationNamespace(applicationNamespace)
                .WithBuildName(buildName)
                .HasApplicationBuildExtension(applicationExtensions.HasApplicationExtension(buildName, "Microsoft.AspNetCore.Builder.WebApplication", compilation))
                .HasBuilderBuildExtension(builderExtensions.HasApplicationExtension(buildName, "Microsoft.AspNetCore.Builder.WebApplicationBuilder", compilation))
                .HasProgram(compilation.HasProgram())
                .IsExecutable(compilation.Options.OutputKind == OutputKind.ConsoleApplication || compilation.Options.OutputKind == OutputKind.WindowsApplication)
                .IsHost(isHost));
        }
    }
}