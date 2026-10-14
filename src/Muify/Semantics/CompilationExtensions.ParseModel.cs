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

            string[] segments = assemblyName.Split('.');

            if (HasHostSuffix(applicationNamespace) || HasHostSuffix(assemblyName))
            {
                string hostNamespace = HasHostSuffix(applicationNamespace) ? applicationNamespace : assemblyName;
                string[] hostSegments = hostNamespace.Split('.');
                string buildName = $"Build{hostSegments[hostSegments.Length - 2]}";
                Model host = Model.Undefined.For(hostSegments[0]).Named(hostSegments[1]);

                return compilation.WithApplicationMetadata(host, assemblies, applicationNamespace, buildName, true, compilation.GetHostedFeatures());
            }

            const int MinimumSegments = 4;

            if (segments.Length < MinimumSegments)
            {
                return compilation.WithApplicationMetadata(Model.Undefined, assemblies, applicationNamespace, string.Empty, false, ImmutableArray<Qualification>.Empty);
            }

            Name company = segments[0];
            Name name = segments[1];

            Feature feature = Feature.Undefined;

            const int FeatureSegments = 5;

            if (segments.Length == FeatureSegments)
            {
                feature = compilation.ParseFeatureModel((Area: segments[2], Feature: segments[4], Unit: segments[3]));
            }

            Model model = compilation
                .ParseDomainModel(feature, (Area: segments[2], Unit: segments[3]), cancellationToken)
                .For(company)
                .Named(name);

            ImmutableArray<Qualification> features = feature.IsUndefined
                ? ImmutableArray<Qualification>.Empty
                : ImmutableArray.Create<Qualification>((Name: segments[segments.Length - 1], Qualifier: assemblyName));
            bool canBuild = feature.Metadata.HasBuildExtension
                || feature.Metadata.HasBuilderBuildExtension
                || ((feature.Metadata.IsPartial || feature.Metadata.HasRegistrar)
                    && GenerateRegisterModeWhenFeatureVisited.CanGenerate(feature));
            string featureBuildName = !feature.IsUndefined && canBuild ? $"Build{feature.Name}" : string.Empty;

            return compilation.WithApplicationMetadata(model, assemblies, feature.IsUndefined ? applicationNamespace : assemblyName, featureBuildName, false, features);
        }
    }
}