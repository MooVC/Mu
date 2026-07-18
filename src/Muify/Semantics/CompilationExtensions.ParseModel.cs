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

    internal static partial class CompilationExtensions
    {
        private const string CompositionAssemblyName = "Mu.Composition";

        internal static Model ParseModel(this Compilation compilation, CancellationToken cancellationToken)
        {
            string assemblyName = compilation.AssemblyName ?? string.Empty;

            var assemblies = compilation.ReferencedAssemblyNames
                .Select(reference => reference.Name)
                .ToImmutableArray();

            string[] segments = assemblyName.Split('.');

            const int MinimumSegments = 4;

            if (segments.Length < MinimumSegments)
            {
                return WithMetadata(Model.Undefined, assemblies);
            }

            Name company = segments[0];
            Name name = segments[1];

            Model model = Model.Undefined
                .For(company)
                .Named(name);

            const int DomainSegments = 4;

            if (segments.Length < DomainSegments)
            {
                return WithMetadata(model, assemblies);
            }

            Feature feature = Feature.Undefined;

            const int FeatureSegments = 5;

            if (segments.Length == FeatureSegments)
            {
                feature = compilation.ParseFeatureModel((Area: segments[2], Feature: segments[4], Unit: segments[3]));
            }

            model = compilation.ParseDomainModel(feature, (Area: segments[2], Unit: segments[3]), cancellationToken);

            return WithMetadata(model, assemblies);
        }

        private static Model WithMetadata(Model model, ImmutableArray<string> assemblies)
        {
            return model.WithMetadata(metadata => metadata
                .HasComposition(assemblies.Contains(CompositionAssemblyName))
                .Enumerate((assembly, subject) => subject.WithAssemblies(assembly), assemblies));
        }
    }
}