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
    using static Muify.Semantics.INamedTypeSymbolExtensions;

    internal static partial class CompilationExtensions
    {
        private static ImmutableArray<Qualification> GetHostedFeatures(this Compilation compilation)
        {
            return compilation.References
                .Where(IsGlobalReference)
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .SelectMany(assembly => assembly.GlobalNamespace.GetAllTypes())
                .Where(request => IsHostedFeature(request, compilation))
                .Select(request => request.ToQualification())
                .Distinct()
                .OrderBy(feature => $"{feature.Qualifier}.{feature.Moniker}", StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static bool HasFeatureApplicationBuildMethod(INamedTypeSymbol request, Compilation compilation)
        {
            return request.ContainingNamespace.GetTypeMembers("WebApplicationExtensions").Any(extensions =>
                extensions.HasApplicationExtension($"Build{request.Name}", "Microsoft.AspNetCore.Builder.WebApplication", compilation));
        }

        private static bool HasHostSuffix(string projectNamespace)
        {
            string[] segments = projectNamespace.Split('.');

            return segments.Length >= 2 && segments[segments.Length - 1] == "Host";
        }

        private static bool HasProgram(this Compilation compilation)
        {
            return compilation.GetEntryPoint(CancellationToken.None) is object
                || compilation.SyntaxTrees.Any(IsProgramSource);
        }

        private static bool IsExecutableOutput(OutputKind output)
        {
            return output == OutputKind.ConsoleApplication || output == OutputKind.WindowsApplication;
        }

        private static bool IsFeatureProjectRecord(INamedTypeSymbol request)
        {
            const int MinimumFeatureSegments = 5;

            return request.Name == request.ContainingNamespace.Name
                && !HasHostSuffix(request.ContainingNamespace.ToDisplayString())
                && request.ContainingNamespace.ToDisplayString().Split('.').Length >= MinimumFeatureSegments
                && request.GetProjectKind() == ProjectKind.Feature;
        }

        private static bool IsGlobalReference(MetadataReference reference)
        {
            return reference.Properties.Aliases.IsDefaultOrEmpty || reference.Properties.Aliases.Contains("global");
        }

        private static bool IsHostedFeature(INamedTypeSymbol request, Compilation compilation)
        {
            return IsFeatureProjectRecord(request)
                && compilation.IsSymbolAccessibleWithin(request, compilation.Assembly)
                && HasFeatureApplicationBuildMethod(request, compilation);
        }

        private static bool IsProgramSource(SyntaxTree tree)
        {
            return string.Equals(Path.GetFileName(tree.FilePath), "Program.cs", StringComparison.OrdinalIgnoreCase)
                || tree.GetRoot().DescendantNodes().OfType<GlobalStatementSyntax>().Any();
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
                .IsExecutable(IsExecutableOutput(compilation.Options.OutputKind))
                .IsHost(isHost));
        }
    }
}