namespace Muify.Semantics
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Service;
    using static Muify.Semantics.INamedTypeSymbolExtensions;

    internal static partial class CompilationExtensions
    {
        internal static Model ParseModel(this Compilation compilation, CancellationToken cancellationToken)
        {
            return compilation.ParseModel(string.Empty, cancellationToken);
        }

        internal static Model ParseModel(this Compilation compilation, string rootNamespace, CancellationToken cancellationToken)
        {
            string assemblyName = compilation.AssemblyName ?? string.Empty;
            string applicationNamespace = string.IsNullOrWhiteSpace(rootNamespace) ? assemblyName : rootNamespace;

            var assemblies = compilation.ReferencedAssemblyNames
                .Select(reference => reference.Name)
                .ToImmutableArray();

            string hostNamespace = HasHostSuffix(applicationNamespace) ? applicationNamespace : assemblyName;

            if (HasHostSuffix(hostNamespace))
            {
                string[] hostSegments = hostNamespace.Split('.');
                string buildName = $"Build{hostSegments[hostSegments.Length - 2]}";
                Model host = Model.Undefined
                    .For(hostSegments[0])
                    .Named(hostSegments[1])
                    .ScopedWithin(GetModelScope(hostSegments, hostSegments.Length - 3));

                return compilation.WithApplicationMetadata(host, assemblies, hostNamespace, buildName, true, compilation.GetHostedFeatures());
            }

            INamedTypeSymbol definition = compilation.GetProjectDefinition(applicationNamespace);
            ProjectKind kind = definition.GetProjectKind();

            if (kind == ProjectKind.Undefined && applicationNamespace != assemblyName)
            {
                definition = compilation.GetProjectDefinition(assemblyName);
                kind = definition.GetProjectKind();
            }

            string projectNamespace = definition?.ContainingNamespace.ToDisplayString() ?? applicationNamespace;
            string[] segments = projectNamespace.Split('.');

            const int MinimumUnitSegments = 4;
            const int MinimumFeatureSegments = 5;

            bool isFeature = kind == ProjectKind.Feature && segments.Length >= MinimumFeatureSegments;
            bool isUnit = kind == ProjectKind.Unit && segments.Length >= MinimumUnitSegments;

            if (!isFeature && !isUnit)
            {
                return compilation.WithApplicationMetadata(Model.Undefined, assemblies, applicationNamespace, string.Empty, false, ImmutableArray<Qualification>.Empty);
            }

            Name company = segments[0];
            Name name = segments[1];
            int areaIndex = segments.Length - (isFeature ? 3 : 2);
            int unitIndex = areaIndex + 1;
            Feature feature = Feature.Undefined;

            if (isFeature)
            {
                feature = compilation.ParseFeatureModel((Area: segments[areaIndex], Feature: definition.Name, Unit: segments[unitIndex]), definition);
            }

            string domainNamespace = isFeature
                ? projectNamespace.Substring(0, projectNamespace.LastIndexOf('.'))
                : projectNamespace;

            Model model = compilation
                .ParseDomainModel(feature, (Area: segments[areaIndex], Unit: segments[unitIndex]), cancellationToken, domainNamespace);

            if (model.IsUndefined)
            {
                return compilation.WithApplicationMetadata(Model.Undefined, assemblies, projectNamespace, string.Empty, false, ImmutableArray<Qualification>.Empty);
            }

            model = model.For(company)
                .Named(name)
                .ScopedWithin(GetModelScope(segments, areaIndex));

            ImmutableArray<Qualification> features = feature.IsUndefined
                ? ImmutableArray<Qualification>.Empty
                : ImmutableArray.Create(definition.ToQualification());
            bool canBuild = feature.Metadata.HasBuildExtension
                || feature.Metadata.HasBuilderBuildExtension
                || ((feature.Metadata.IsPartial || feature.Metadata.HasRegistrar)
                    && GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature));
            string featureBuildName = !feature.IsUndefined && canBuild ? $"Build{feature.Name}" : string.Empty;

            return compilation.WithApplicationMetadata(model, assemblies, projectNamespace, featureBuildName, false, features);
        }

        private static Qualifier GetModelScope(string[] segments, int areaIndex)
        {
            return new Qualifier(segments.Skip(2).Take(areaIndex - 2).Select(segment => (Name)segment).ToImmutableArray());
        }
    }
}