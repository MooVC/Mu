namespace Muify.Semantics
{
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        internal static Model ParseModel(this Compilation compilation, CancellationToken cancellationToken)
        {
            string assemblyName = compilation.AssemblyName ?? string.Empty;
            string[] segments = assemblyName.Split('.');

            const int MinimumSegments = 4;

            if (segments.Length < MinimumSegments)
            {
                return Model.Undefined;
            }

            Name company = segments[0];
            Name name = segments[1];

            Model model = Model.Undefined
                .For(company)
                .Named(name);

            const int DomainSegments = 4;

            if (segments.Length < DomainSegments)
            {
                return model;
            }

            Feature feature = Feature.Undefined;

            const int FeatureSegments = 5;

            if (segments.Length == FeatureSegments)
            {
                feature = compilation.ParseFeatureModel((Area: segments[2], Feature: segments[4], Unit: segments[3]));
            }

            return compilation.ParseDomainModel(feature, (Area: segments[2], Unit: segments[3]), cancellationToken);
        }
    }
}